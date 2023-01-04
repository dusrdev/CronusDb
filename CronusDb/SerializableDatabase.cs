using System.Security.Cryptography;
using System.Text.Json;

namespace CronusDb;

public sealed class SerializableDatabase<T> : CronusDatabase<T> {
    private readonly Dictionary<string, string> _data;
    private readonly SerializableDatabaseConfiguration<T>? _config;

    // This constructor is used for a serializable instance
    internal SerializableDatabase(Dictionary<string, string> data, SerializableDatabaseConfiguration<T> config) {
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
    public override void Upsert(string key, T value) {
        _data[key] = _config!.ToStringConverter.Invoke(value);
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
            var converted = _config!.FromStringConverter.Invoke(v);
            if (selector.Invoke(converted)) {
                Remove(k);
            }
        }
    }

    /// <summary>
    /// Serializes the database to the path in <see cref="SerializableDatabaseConfiguration{T}"/>.
    /// </summary>
    public override async Task Serialize() {
        if (string.IsNullOrWhiteSpace(_config!.EncryptionKey)) {
            await SerializeWithoutEncryption(_data);
            return;
        }
        await SerializeWithEncryption(_data);
    }

    internal override async Task SerializeWithEncryption(Dictionary<string, string> data) {
        using var aes = new CronusAesProvider(_config!.EncryptionKey!);
        using var encrypter = aes.GetEncrypter();
        using var fileStream = new FileStream(_config!.Path, FileMode.OpenOrCreate);
        using var cryptoStream = new CryptoStream(fileStream, encrypter, CryptoStreamMode.Write);
        using var streamWriter = new StreamWriter(cryptoStream);
        var json = JsonSerializer.Serialize(_data, JsonContexts.Default.DictionaryStringString);
        await streamWriter.WriteAsync(json);
    }

    internal override async Task SerializeWithoutEncryption(Dictionary<string, string> data) {
        using var stream = new FileStream(_config!.Path, FileMode.Create);
        await JsonSerializer.SerializeAsync(stream, _data, JsonContexts.Default.DictionaryStringString);
    }
}
