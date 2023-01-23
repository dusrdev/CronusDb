﻿using MemoryPack;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace CronusDb;

/// <summary>
/// Base type for the generic value database
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TSerialized"></typeparam>
public abstract class GenericDatabase<TValue, TSerialized> {
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
    public abstract TValue? Get(string key);

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> with the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public abstract void Upsert(string key,[DisallowNull] TValue value);

    /// <summary>
    /// An indexer option for <see cref="Get(string)"/> and <see cref="Upsert(string, TValue)"/>.
    /// </summary>
    /// <param name="index"></param>
    public virtual TValue? this[string index] {
        get => Get(index);
        set => Upsert(index, value!);
    }

    /// <summary>
    /// Removes the <paramref name="key"/> and its value from the inner dictionary.
    /// </summary>
    /// <param name="key"></param>
    public abstract bool Remove(string key);

    /// <summary>
    /// Removes all the keys and values in the dictionary for which the value passes the <paramref name="selector"/>.
    /// </summary>
    /// <param name="selector"></param>
    public abstract void RemoveAny(Func<TValue, bool> selector);

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public abstract Task SerializeAsync();

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public abstract void Serialize();

    internal virtual void SerializeWithEncryption(ConcurrentDictionary<string, TSerialized> data, GenericDatabaseConfiguration<TValue, TSerialized> config) {
        var bin = MemoryPackSerializer.Serialize(data);
        var encrypted = bin.Encrypt(config.EncryptionKey!);
        File.WriteAllBytes(config.Path, encrypted);
    }

    internal virtual async Task SerializeWithEncryptionAsync(ConcurrentDictionary<string, TSerialized> data, GenericDatabaseConfiguration<TValue, TSerialized> config) {
        var bin = MemoryPackSerializer.Serialize(data);
        var encrypted = bin.Encrypt(config.EncryptionKey!);
        await File.WriteAllBytesAsync(config.Path, encrypted);
    }

    internal virtual void SerializeWithoutEncryption(ConcurrentDictionary<string, TSerialized> data, GenericDatabaseConfiguration<TValue, TSerialized> config) {
        var bin = MemoryPackSerializer.Serialize(data);
        File.WriteAllBytes(config.Path, bin);
    }

    internal virtual async Task SerializeWithoutEncryptionAsync(ConcurrentDictionary<string, TSerialized> data, GenericDatabaseConfiguration<TValue, TSerialized> config) {
        using var stream = File.OpenWrite(config.Path);
        await MemoryPackSerializer.SerializeAsync(stream, data);
    }
}