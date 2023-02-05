using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;

namespace CronusDb;

/// <summary>
/// Very efficient key-value-pair database with O(1) CRUD but O(N) Serialization.
/// </summary>
/// <typeparam name="TValue">Value type</typeparam>
public sealed class Database<TValue> {
    private readonly ConcurrentDictionary<string, TValue> _data;

    /// <summary>
    /// Holds the configuration for this database.
    /// </summary>
    public readonly DatabaseConfiguration<TValue> Config;

    /// <summary>
    /// Triggered when there is a change in the database.
    /// </summary>
    public event EventHandler<DataChangedEventArgs>? DataChanged;

    private void OnDataChanged(DataChangedEventArgs e) {
        DataChanged?.Invoke(this, e);
    }

    // This constructor is used for a serializable instance
    internal Database(ConcurrentDictionary<string, TValue>? data, DatabaseConfiguration<TValue> config) {
        _data = data ?? new();
        Config = config;
    }

    /// <summary>
    /// Returns the amount of entries in the database
    /// </summary>
    public int Count => _data.Count;

    /// <summary>
    /// Returns a immutable list of all keys in the database.
    /// </summary>
    public ImmutableList<string> GetKeys() => ImmutableList.CreateRange(_data.Keys);

    /// <summary>
    /// Returns an immutable copy of the inner dictionary
    /// </summary>
    public ImmutableDictionary<string, TValue> GetInnerDict() => ImmutableDictionary.CreateRange(_data);

    /// <summary>
    /// Indexer option for the getter and setter.
    /// </summary>
    /// <param name="index"></param>
    /// <remarks>
    /// Unlike the <see cref="Dictionary{TKey, TValue}"/> this indexer will return default value if the key doesn't exist instead of throwing an exception.
    /// </remarks>
    public TValue? this[string index] {
        get => Get(index);
        set => Upsert(index, value!);
    }

    /// <summary>
    /// Returns the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    public TValue? Get(string key) => _data.TryGetValue(key, out var val) ? val : default;

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> @ <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <remarks>
    /// Do not upsert null values, they most likely will cause issues with serialization.
    /// </remarks>
    public void Upsert(string key, TValue value) {
        Debug.Assert(value is not null);
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

    /// <summary>
    /// Checked whether the inner dictionary contains the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    public bool ContainsKey(string key) => _data.ContainsKey(key);

    /// <summary>
    /// Removes the <paramref name="key"/> and its value from the inner dictionary.
    /// </summary>
    /// <param name="key"></param>
    public bool Remove(string key) {
        if (_data.IsEmpty) {
            if (Config.Options.HasFlag(DatabaseOptions.TriggerUpdateEvents)) {
                OnDataChanged(new DataChangedEventArgs {
                    Key = key,
                    Value = null,
                    ChangeType = DataChangeType.Remove
                });
            }
            return true;
        }

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

    /// <summary>
    /// Removes all the keys and values in the dictionary for which the value passes the <paramref name="selector"/>.
    /// </summary>
    /// <param name="selector"></param>
    public void RemoveAny(Func<TValue, bool> selector) {
        Debug.Assert(selector is not null);
        Debug.Assert(!_data.IsEmpty);
        if (_data.IsEmpty) {
            return;
        }
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

    /// <summary>
    /// Saves the database to the hard disk asynchronously.
    /// </summary>
    public async Task SerializeAsync() {
        var output = _data.Convert(Config!.ToTSerialized);

        await output.SerializeAsync(Config!.Path, Config!.EncryptionKey);
    }

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public void Serialize() {
        var output = _data.Convert(Config!.ToTSerialized);

        output.Serialize(Config!.Path, Config!.EncryptionKey);
    }
}
