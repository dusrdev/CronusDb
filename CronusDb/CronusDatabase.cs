using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
namespace CronusDb;

/// <summary>
/// Serializable database which supports per key encryption
/// </summary>
public sealed class CronusDatabase : Database {
    private readonly ConcurrentDictionary<string, string> _data;
    private readonly DatabaseConfiguration _config;

    internal CronusDatabase(ConcurrentDictionary<string, string>? data, DatabaseConfiguration config) {
        _data = data ?? new();
        _config = config;
    }

    internal override void OnDataChanged(DataChangedEventArgs e) {
        base.OnDataChanged(e);
        if (_config.SerializeOnUpdate) {
            Serialize();
        }
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
    public override bool Remove(string key) {
        if (!_data.TryRemove(key, out var val)) {
            return false;
        }
        OnDataChanged(new DataChangedEventArgs {
            Key = key,
            Value = val,
            ChangeType = DataChangeType.Remove
        });
        return true;
    }

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public override void Serialize() => Serializer.Serialize(_data, _config!.Path, _config!.EncryptionKey);

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public override Task SerializeAsync() => Serializer.SerializeAsync(_data, _config!.Path, _config!.EncryptionKey);

    /// <summary>
    /// Updates or inserts a new <paramref name="value" /> with the <paramref name="key" />.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <exception cref="ArgumentException"/>
    public override void Upsert(string key, [DisallowNull] string value, string? encryptionKey = null) {
        ArgumentException.ThrowIfNullOrEmpty(value);
        _data[key] = encryptionKey is null ? value : value.Encrypt(encryptionKey);
        OnDataChanged(new DataChangedEventArgs {
            Key = key,
            Value = value,
            ChangeType = DataChangeType.Upsert
        });
    }
}
