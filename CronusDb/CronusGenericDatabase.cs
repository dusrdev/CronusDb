using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace CronusDb;

/// <summary>
/// Fast Crud performance at the cost of slower serialization to disk.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class CronusGenericDatabase<T> : GenericDatabase<T> {
    private readonly ConcurrentDictionary<string, T> _data;
    private readonly GenericDatabaseConfiguration<T> _config;

    // This constructor is used for a serializable instance
    internal CronusGenericDatabase(ConcurrentDictionary<string, T> data, GenericDatabaseConfiguration<T> config) {
        _data = data;
        _config = config;
    }

    internal override void OnDataChanged(DataChangedEventArgs e) {
        base.OnDataChanged(e);
        if (_config.SerializeOnUpdate) {
            Serialize();
        }
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
    /// <exception cref="ArgumentNullException"/>
    public override void Upsert(string key, [DisallowNull] T value) {
        ArgumentNullException.ThrowIfNull(value);
        var isUpdate = _data.ContainsKey(key);
        _data[key] = value;
        OnDataChanged(new DataChangedEventArgs {
            Key = key,
            Value = value,
            ChangeType = isUpdate ? DataChangeType.Update : DataChangeType.Insert
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
    public override void RemoveAny(Func<T, bool> selector) {
        int count = 0;
        foreach (var (k, v) in _data) {
            if (!selector.Invoke(v)) {
                continue;
            }
            if (!_data.TryRemove(k, out _)) {
                continue;
            }
            count++;
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
    /// Serializes the database to the path in <see cref="GenericDatabaseConfiguration{T}"/>.
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
    /// Serializes the database to the path in <see cref="GenericDatabaseConfiguration{T}"/>.
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
