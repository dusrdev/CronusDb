﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CronusDb;

/// <summary>
/// Base type for the polymorphic database
/// </summary>
public abstract class Database {
    /// <summary>
    /// Triggered when there is a change in the database.
    /// </summary>
    public event EventHandler<DataChangedEventArgs>? DataChanged;

    internal virtual void OnDataChanged(DataChangedEventArgs e) {
        DataChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Checked whether the inner dictionary contains the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    public abstract bool ContainsKey(string key);

    /// <summary>
    /// Returns the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    public abstract string? Get(string key, string? encryptionKey = null);

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> with the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    public abstract void Upsert(string key, [DisallowNull] string value, string? encryptionKey = null);

    /// <summary>
    /// Removes the <paramref name="key"/> and its value from the inner dictionary.
    /// </summary>
    /// <param name="key"></param>
    public abstract bool Remove(string key);

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public abstract Task SerializeAsync();

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public abstract void Serialize();

    internal virtual void SerializeWithEncryption(IDictionary<string, string> data, DatabaseConfiguration config) {
        var json = JsonSerializer.Serialize(data, JsonContexts.Default.IDictionaryStringString);
        var encrypted = json.Encrypt(config.EncryptionKey!);
        File.WriteAllText(config.Path, encrypted);
    }

    internal virtual async Task SerializeWithEncryptionAsync(IDictionary<string, string> data, DatabaseConfiguration config) {
        var json = JsonSerializer.Serialize(data, JsonContexts.Default.IDictionaryStringString);
        var encrypted = json.Encrypt(config.EncryptionKey!);
        await File.WriteAllTextAsync(config.Path, encrypted);
    }

    internal virtual void SerializeWithoutEncryption(IDictionary<string, string> data, DatabaseConfiguration config) {
        var json = JsonSerializer.Serialize(data, JsonContexts.Default.IDictionaryStringString);
        File.WriteAllText(config.Path, json);
    }

    internal virtual async Task SerializeWithoutEncryptionAsync(IDictionary<string, string> data, DatabaseConfiguration config) {
        var json = JsonSerializer.Serialize(data, JsonContexts.Default.IDictionaryStringString);
        await File.WriteAllTextAsync(config.Path, json);
    }
}