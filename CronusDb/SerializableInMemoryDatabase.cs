namespace CronusDb;

public sealed class SerializableInMemoryDatabase<T> : CronusDatabase<T> {
    private readonly Dictionary<string, T> _data;
    private readonly SerializableDatabaseConfiguration<T>? _config;

    // This constructor is used for a serializable instance
    internal SerializableInMemoryDatabase(Dictionary<string, T> data, SerializableDatabaseConfiguration<T> config) {
        _data = data;
        _config = config;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise the default for <typeparamref name="T"/>.
    /// </summary>
    /// <param name="key"></param>
    public override T? Get(string key) => _data.TryGetValue(key, out var value) ? value : default;

    /// <summary>
    /// Inserts or updates
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public override void Upsert(string key, T value) {
        _data[key] = value;
        OnItemUpserted(EventArgs.Empty);
    }

    /// <summary>
    /// Checks whether the database contains the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    public override bool ContainsKey(string key) => _data.ContainsKey(key);

    /// <summary>
    /// Removes a value by <paramref name="key"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns>True if successful, false if database did not contain <paramref name="key"/>.</returns>
    public override bool Remove(string key) {
        var output = _data.Remove(key);
        if (output) {
            OnItemRemoved(EventArgs.Empty);
        }
        return output;
    }

    /// <summary>
    /// Loops through all database entries and removes any for which the <paramref name="selector"/> returns true.
    /// </summary>
    /// <param name="selector"></param>
    /// <remarks>
    /// This is the main way to perform cache invalidation.
    /// </remarks>
    public override void RemoveAny(Func<T, bool> selector) {
        foreach (var (k, v) in _data) {
            if (selector.Invoke(v)) {
                Remove(k);
            }
        }
    }

    /// <summary>
    /// Serializes the database to the path in <see cref="SerializableDatabaseConfiguration{T}"/>.
    /// </summary>
    public override async Task Serialize() {
        var output = new Dictionary<string, string>();

        foreach (var (k, v) in _data) {
            output.Add(k, _config!.ToStringConverter(v));
        }

        if (string.IsNullOrWhiteSpace(_config!.EncryptionKey)) {
            await SerializeWithoutEncryption(output, _config!);
            return;
        }
        await SerializeWithEncryption(output, _config);
    }
}
