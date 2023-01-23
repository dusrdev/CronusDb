using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace CronusDb;

/// <summary>
/// Best performance at cost of value serialization convenience.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <remarks>
/// Requires a way to deserialize and serialize the generic value to byte[]
/// </remarks>
public sealed class CronusBinaryDatabase<TValue> : Database<TValue> {
    private readonly ConcurrentDictionary<string, TValue> _data;
    private readonly GenericDatabaseConfiguration<TValue, byte[]> _config;

    // This constructor is used for a serializable instance
    internal CronusBinaryDatabase(ConcurrentDictionary<string, TValue>? data, GenericDatabaseConfiguration<TValue, byte[]> config) {
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
    /// Returns the value if it exists, otherwise the default for <typeparamref name="TValue"/>.
    /// </summary>
    /// <param name="key"></param>
    public override TValue? Get(string key) => _data.GetValueOrDefault(key);

    /// <summary>
    /// Inserts or updates
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <exception cref="ArgumentNullException"/>
    public override void Upsert(string key, [DisallowNull] TValue value) {
        ArgumentNullException.ThrowIfNull(value);
        _data[key] = value;
        OnDataChanged(new DataChangedEventArgs {
            Key = key,
            Value = value,
            ChangeType = DataChangeType.Upsert
        });
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
    /// Loops through all database entries and removes any for which the <paramref name="selector"/> returns true.
    /// </summary>
    /// <param name="selector"></param>
    /// <remarks>
    /// This is the main way to perform cache invalidation.
    /// </remarks>
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
            base.OnDataChanged(new DataChangedEventArgs() {
                Key = k,
                Value = v,
                ChangeType = DataChangeType.Remove
            });
        }
        if (count is 0 || !_config.SerializeOnUpdate) {
            return;
        }
        Serialize();
    }

    /// <summary>
    /// Serializes the database to the path in <see cref="GenericDatabaseConfiguration{TValue, TSerialized}"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the value of certain key could not be converted using the "ToStringConverter"</exception>
    public override async Task SerializeAsync() {
        var output = _data.Convert(_config!.ToTSerialized);

        await Serializer.SerializeAsync(output, _config!.Path, _config.EncryptionKey);
    }

    /// <summary>
    /// Serializes the database to the path in <see cref="GenericDatabaseConfiguration{TValue, TSerialized}"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the value of certain key could not be converted using the "ToStringConverter"</exception>
    public override void Serialize() {
        var output = _data.Convert(_config!.ToTSerialized);

        Serializer.Serialize(output, _config!.Path, _config.EncryptionKey);
    }
}
