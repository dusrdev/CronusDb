using System.Collections.Concurrent;

namespace CronusDb;

/// <summary>
/// Fast Crud performance at the cost of slower serialization to disk.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class SerializableInMemoryDatabase<T> : CronusBase<T> {
    private readonly ConcurrentDictionary<string, T> _data;
    private readonly SerializableDatabaseConfiguration<T>? _config;

    // This constructor is used for a serializable instance
    internal SerializableInMemoryDatabase(ConcurrentDictionary<string, T> data, SerializableDatabaseConfiguration<T> config) {
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
        var output = _data.Remove(key, out _);
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
    /// <exception cref="InvalidOperationException">If the value of certain key could not be converted using the "ToStringConverter"</exception>
    public override async Task SerializeAsync() {
        var output = new Dictionary<string, string>();

        foreach (var (k, v) in _data) {
            var str = string.Empty;
            try {
                str = _config!.ToStringConverter(v);
            } catch {
                throw new InvalidOperationException($"Converting the value of key <{k}> failed.");
            }
            output.Add(k, str);
        }

        if (string.IsNullOrWhiteSpace(_config!.EncryptionKey)) {
            await SerializeWithoutEncryptionAsync(output, _config!);
            return;
        }
        await SerializeWithEncryptionAsync(output, _config);
    }

    /// <summary>
    /// Serializes the database to the path in <see cref="SerializableDatabaseConfiguration{T}"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the value of certain key could not be converted using the "ToStringConverter"</exception>
    public override void Serialize() {
        var output = new Dictionary<string, string>();

        foreach (var (k, v) in _data) {
            var str = string.Empty;
            try {
                str = _config!.ToStringConverter(v);
            } catch {
                throw new InvalidOperationException($"Converting the value of key <{k}> failed.");
            }
            output.Add(k, str);
        }

        if (string.IsNullOrWhiteSpace(_config!.EncryptionKey)) {
            SerializeWithoutEncryption(output, _config!);
            return;
        }
        SerializeWithEncryption(output, _config);
    }
}
