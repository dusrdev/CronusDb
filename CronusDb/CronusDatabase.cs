using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;

namespace CronusDb;

/// <summary>
/// Base type for the database
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class CronusDatabase<T> {
    /// <summary>
    /// Triggered when an item is upserted.
    /// </summary>
    /// <remarks>
    /// You can use this to hook cache invalidation with <see cref="RemoveAny(Func{T, bool})"/>.
    /// </remarks>
    public event EventHandler? ItemUpserted;

    internal virtual void OnItemUpserted(EventArgs e) {
        ItemUpserted?.Invoke(this, e);
    }

    /// <summary>
    /// Triggered when an item is removed.
    /// </summary>
    /// <remarks>
    /// You can use this to hook cache invalidation with <see cref="RemoveAny(Func{T, bool})"/>.
    /// </remarks>
    public event EventHandler? ItemRemoved;

    internal virtual void OnItemRemoved(EventArgs e) {
        ItemRemoved?.Invoke(this, e);
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
    public abstract T? Get(string key);

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> with the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public abstract void Upsert(string key,[DisallowNull] T value);

    /// <summary>
    /// An indexer option for <see cref="Get(string)"/> and <see cref="Upsert(string, T)"/>.
    /// </summary>
    /// <param name="index"></param>
    public virtual T? this[string index] {
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
    public abstract void RemoveAny(Func<T, bool> selector);

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public abstract Task SerializeAsync();

    internal virtual async Task SerializeWithEncryptionAsync(IDictionary<string, string> data, SerializableDatabaseConfiguration<T> config) {
        using var aes = new CronusAesProvider(config.EncryptionKey!);
        using var encrypter = aes.GetEncrypter();
        using var fileStream = new FileStream(config.Path, FileMode.OpenOrCreate);
        using var cryptoStream = new CryptoStream(fileStream, encrypter, CryptoStreamMode.Write);
        using var streamWriter = new StreamWriter(cryptoStream);
        var json = JsonSerializer.Serialize(data, JsonContexts.Default.IDictionaryStringString);
        await streamWriter.WriteAsync(json);
    }

    internal virtual async Task SerializeWithoutEncryptionAsync(IDictionary<string, string> data, SerializableDatabaseConfiguration<T> config) {
        using var stream = new FileStream(config.Path, FileMode.Create);
        await JsonSerializer.SerializeAsync(stream, data, JsonContexts.Default.IDictionaryStringString);
    }
}
