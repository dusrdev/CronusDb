using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
namespace CronusDb;

/// <summary>
/// Serializable database which supports per key encryption
/// </summary>
public sealed class PolymorphicDatabase : PolymorphicBase {
    private readonly ConcurrentDictionary<string, string> _data;
    private readonly PolymorphicConfiguration _config;

    internal PolymorphicDatabase(ConcurrentDictionary<string, string> data, PolymorphicConfiguration config) {
        _data = data;
        _config = config;
    }

    /// <summary>
    /// Checked whether the inner dictionary contains the <paramref name="key" />.
    /// </summary>
    /// <param name="key"></param>
    public override bool ContainsKey(string key) => _data.ContainsKey(key);

    /// <summary>
    /// Returns the value for the <paramref name="key" />.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    public override string? Get(string key, string? encryptionKey = null) {
        if (!_data.TryGetValue(key, out var val)) {
            return null;
        }
        if (encryptionKey is null) {
            return val;
        }
        return val.Decrypt(encryptionKey);
    }

    /// <summary>
    /// Removes the <paramref name="key" /> and its value from the inner dictionary.
    /// </summary>
    /// <param name="key"></param>
    public override bool Remove(string key) => _data.Remove(key, out _);

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public override void Serialize() {
        if (string.IsNullOrWhiteSpace(_config!.EncryptionKey)) {
            SerializeWithoutEncryption(_data, _config);
            return;
        }
        SerializeWithEncryption(_data, _config);
    }

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public override async Task SerializeAsync() {
        if (string.IsNullOrWhiteSpace(_config!.EncryptionKey)) {
            await SerializeWithoutEncryptionAsync(_data, _config);
            return;
        }
        await SerializeWithEncryptionAsync(_data, _config);
    }

    /// <summary>
    /// Updates or inserts a new <paramref name="value" /> with the <paramref name="key" />.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    public override void Upsert(string key, [DisallowNull] string value, string? encryptionKey = null) {
        ArgumentException.ThrowIfNullOrEmpty(value);
        if (encryptionKey is null) {
            _data[key] = value;
            return;
        }
        _data[key] = value.Encrypt(encryptionKey);
    }
}
