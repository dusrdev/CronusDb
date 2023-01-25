using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
namespace CronusDb;

/// <summary>
/// Serializable database which supports per key encryption
/// </summary>
public sealed class CronusDatabase : Database {
    private readonly ConcurrentDictionary<string, string> _data;

    /// <inheritdoc/>
    public override DatabaseConfiguration Config { get; protected init; }

    internal CronusDatabase(ConcurrentDictionary<string, string>? data, DatabaseConfiguration config) {
        _data = data ?? new();
        Config = config;
    }

    /// <inheritdoc/>
    public override bool ContainsKey(string key) => _data.ContainsKey(key);

    /// <inheritdoc/>
    public override string? Get(string key, string? encryptionKey = null) {
        if (!_data.TryGetValue(key, out var val)) {
            return null;
        }
        if (encryptionKey is null) {
            return val;
        }
        return val.Decrypt(encryptionKey);
    }

    /// <inheritdoc/>
    public override bool Remove(string key) {
        if (!_data.TryRemove(key, out var val)) {
            return false;
        }
        if (Config.Options.HasFlag(DatabaseOptions.SerializeOnUpdate)) {
            Serialize();
        }
        if (Config.Options.HasFlag(DatabaseOptions.TriggerUpdateEvents)) {
            OnDataChanged(new DataChangedEventArgs {
                Key = key,
                Value = val,
                ChangeType = DataChangeType.Remove
            });
        }
        return true;
    }

    /// <inheritdoc/>
    public override void Serialize() => _data.Serialize(Config!.Path, Config!.EncryptionKey);

    /// <inheritdoc/>
    public override Task SerializeAsync() => _data.SerializeAsync(Config!.Path, Config!.EncryptionKey);

    /// <inheritdoc/>
    public override void Upsert(string key, [DisallowNull] string value, string? encryptionKey = null) {
        ArgumentException.ThrowIfNullOrEmpty(value);
        _data[key] = encryptionKey is null ? value : value.Encrypt(encryptionKey);
        if (Config.Options.HasFlag(DatabaseOptions.SerializeOnUpdate)) {
            Serialize();
        }
        if (Config.Options.HasFlag(DatabaseOptions.TriggerUpdateEvents)) {
            OnDataChanged(new DataChangedEventArgs {
                Key = key,
                Value = value,
                ChangeType = DataChangeType.Upsert
            });
        }
    }
}
