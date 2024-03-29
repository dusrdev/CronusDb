﻿using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;

namespace CronusDb;

/// <summary>
/// Serializable database which supports per key encryption
/// </summary>
public sealed class Database {
    private readonly ConcurrentDictionary<string, byte[]> _data;

    /// <summary>
    /// Holds the configuration for this database.
    /// </summary>
    public readonly DatabaseConfiguration Config;

    /// <summary>
    /// Triggered when there is a change in the database.
    /// </summary>
    public event EventHandler<DataChangedEventArgs>? DataChanged;

    private void OnDataChanged(DataChangedEventArgs e) {
        DataChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Default constructor is disallowed. Use Cronus factory methods instead.
    /// </summary>
    public Database() => throw new InvalidOperationException("Use of default constructor is disallowed. Use Cronus factory methods instead.");

    internal Database(ConcurrentDictionary<string, byte[]>? data, DatabaseConfiguration config) {
        _data = data ?? new(config.Options.GetComparer());
        Config = config;
    }

    /// <summary>
    /// Returns the amount of entries in the database.
    /// </summary>
    public int Count => _data.Count;

    /// <summary>
    /// Checked whether the inner dictionary contains the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    public bool ContainsKey(string key) => _data.ContainsKey(key);

    /// <summary>
    /// Returns the value for the <paramref name="key"/> as a byte[].
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <remarks>
    /// <para>This pure method which returns the value as byte[] allows you to use more complex but also more efficient serializers
    /// </para>
    /// <para>If the value doesn't exist null is returned. You can use this to check if a value exists.</para>
    /// </remarks>
    public byte[]? Get(string key, string? encryptionKey = null) {
        if (!_data.TryGetValue(key, out var val)) {
            return null;
        }
        return encryptionKey is null
            ? val
            : val.Decrypt(encryptionKey);
    }

    /// <summary>
    /// Returns the value for the <paramref name="key"/> as string.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    public string? GetAsString(string key, string? encryptionKey = null) => Get(key, encryptionKey)?.ToUTF8String();

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
    /// Clears all keys and values from the database.
    /// </summary>
    public void Clear() {
        _data.Clear();
        if (Config.Options.HasFlag(DatabaseOptions.SerializeOnUpdate)) {
            Serialize();
        }
        if (Config.Options.HasFlag(DatabaseOptions.TriggerUpdateEvents)) {
            OnDataChanged(new DataChangedEventArgs {
                Key = "ALL",
                Value = null,
                ChangeType = DataChangeType.Remove
            });
        }
    }

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> @ <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <remarks>
    /// This pure method which accepts the value as byte[] allows you to use more complex but also more efficient serializers.
    /// </remarks>
    public void Upsert(string key, byte[] value, string? encryptionKey = null) {
        Debug.Assert(value is not null);
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

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> @ <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <remarks>
    /// This is much less efficient time and memory wise than <see cref="Upsert(string, byte[], string?)"/>.
    /// </remarks>
    public void UpsertAsString(string key, string value, string? encryptionKey = null) {
        Debug.Assert(!string.IsNullOrWhiteSpace(value));

        var bytes = string.IsNullOrEmpty(value) ?
                    Array.Empty<byte>()
                    : value.ToByteArray();

        Upsert(key, bytes, encryptionKey);
    }

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> @ <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <remarks>
    /// This is the least efficient option as it uses a reflection JSON serializer and byte conversion.
    /// </remarks>
    public void UpsertAsT<T>(string key, T value, string? encryptionKey = null) {
        Debug.Assert(value is not null);

        var bytes = value is null ?
                    Array.Empty<byte>()
                    : value.Serialize().ToByteArray();

        Upsert(key, bytes, encryptionKey);
    }

    /// <summary>
    /// Returns an immutable copy of the keys in the inner dictionary
    /// </summary>
    public ImmutableList<string> Keys => ImmutableList.CreateRange(_data.Keys);

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public void Serialize() => _data.Serialize(Config!.Path, Config!.EncryptionKey);

    /// <summary>
    /// Saves the database to the hard disk asynchronously.
    /// </summary>
    public Task SerializeAsync() => _data.SerializeAsync(Config!.Path, Config!.EncryptionKey);
}
