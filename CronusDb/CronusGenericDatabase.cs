using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace CronusDb;

/// <summary>
/// Fast Crud performance at the cost of slower serialization to disk.
/// </summary>
/// <typeparam name="TValue">Value type</typeparam>
/// <typeparam name="TSerialized">Value serialization type</typeparam>
public sealed class CronusGenericDatabase<TValue, TSerialized> : Database<TValue, TSerialized> {
    private readonly ConcurrentDictionary<string, TValue> _data;

    /// <inheritdoc/>
    public override GenericDatabaseConfiguration<TValue, TSerialized> Config { get; protected init; }

    // This constructor is used for a serializable instance
    internal CronusGenericDatabase(ConcurrentDictionary<string, TValue>? data, GenericDatabaseConfiguration<TValue, TSerialized> config) {
        _data = data ?? new();
        Config = config;
    }

    /// <inheritdoc/>
    public override TValue? Get(string key) => _data.TryGetValue(key, out var val) ? val : default;

    /// <inheritdoc/>
    public override void Upsert(string key, [DisallowNull] TValue value) {
        ArgumentNullException.ThrowIfNull(value);
        _data[key] = value;
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

    /// <inheritdoc/>
    public override bool ContainsKey(string key) => _data.ContainsKey(key);

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
    public override void RemoveAny(Func<TValue, bool> selector) {
        int count = 0;
        foreach (var (k, v) in _data) {
            if (!selector.Invoke(v)) {
                continue;
            }
            if (!_data.TryRemove(k, out _)) {
                continue;
            }
            count++;
            // Triggering the base version skip the serialization to improve performance for bulk removals.
            if (Config.Options.HasFlag(DatabaseOptions.TriggerUpdateEvents)) {
                OnDataChanged(new DataChangedEventArgs() {
                    Key = k,
                    Value = v,
                    ChangeType = DataChangeType.Remove
                });
            }
        }
        if (count is 0 || !Config.Options.HasFlag(DatabaseOptions.SerializeOnUpdate)) {
            return;
        }
        Serialize();
    }

    /// <inheritdoc/>
    public override async Task SerializeAsync() {
        var output = _data.Convert(Config!.ToTSerialized);

        await output.SerializeAsync(Config!.Path, Config!.EncryptionKey);
    }

    /// <inheritdoc/>
    public override void Serialize() {
        var output = _data.Convert(Config!.ToTSerialized);

        output.Serialize(Config!.Path, Config!.EncryptionKey);
    }
}
