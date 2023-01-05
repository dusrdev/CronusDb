using System.Collections.Concurrent;

namespace CronusDb;

/// <summary>
/// Serializable database, everything is stored after being serialized to improve disk operation performance.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class SerializableDatabase<T> : CronusDatabase<T> {
    private readonly ConcurrentDictionary<string, string> _data;
    private readonly SerializableDatabaseConfiguration<T>? _config;

    // This constructor is used for a serializable instance
    internal SerializableDatabase(ConcurrentDictionary<string, string> data, SerializableDatabaseConfiguration<T> config) {
        _data = data;
        _config = config;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise the default for <typeparamref name="T"/>.
    /// </summary>
    /// <param name="key"></param>
    public override T? Get(string key) => _data.TryGetValue(key, out var value) ? _config!.FromStringConverter.Invoke(value) : default;

    /// <summary>
    /// Inserts or updates
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <exception cref="InvalidOperationException">If the conversion to string using "ToStringConverter" failed./></exception>
    public override void Upsert(string key, T value) {
        var val = string.Empty;
        try {
            val = _config!.ToStringConverter.Invoke(value);
        } catch {
            throw new InvalidOperationException($"Converting the value of key <{key}> failed.");
        }
        _data[key] = val;
        OnItemUpserted(EventArgs.Empty);
    }

    /// <summary>
    /// Checks whether the database contains the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    public override bool ContainsKey(string key) => _data.ContainsKey(key);

    /// <summary>
    /// removes a value by <paramref name="key"/>
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
    /// <exception cref="InvalidOperationException">If the value of certain key could not be converted back using "FromStringConverter"</exception>
    public override void RemoveAny(Func<T, bool> selector) {
        foreach (var (k, v) in _data) {
            var converted = default(T);
            try {
                converted = _config!.FromStringConverter.Invoke(v);
            } catch {
                throw new InvalidOperationException($"Converting the value of key <{k}> failed.");
            }
            if (selector.Invoke(converted)) {
                Remove(k);
            }
        }
    }

    /// <summary>
    /// Serializes the database to the path in <see cref="SerializableDatabaseConfiguration{T}"/>.
    /// </summary>
    public override async Task SerializeAsync() {
        if (string.IsNullOrWhiteSpace(_config!.EncryptionKey)) {
            await SerializeWithoutEncryptionAsync(_data, _config);
            return;
        }
        await SerializeWithEncryptionAsync(_data, _config);
    }

    /// <summary>
    /// Serializes the database to the path in <see cref="SerializableDatabaseConfiguration{T}"/>.
    /// </summary>
    public override void Serialize() {
        if (string.IsNullOrWhiteSpace(_config!.EncryptionKey)) {
            SerializeWithoutEncryption(_data, _config);
            return;
        }
        SerializeWithEncryption(_data, _config);
    }
}
