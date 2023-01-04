using System.Security.Cryptography;
using System.Text.Json;

namespace CronusDb;

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

    public abstract bool ContainsKey(string key);

    public abstract T? Get(string key);

    public abstract void Upsert(string key, T value);

    public abstract bool Remove(string key);

    public abstract void RemoveAny(Func<T, bool> selector);

    public abstract Task Serialize();

    internal virtual async Task SerializeWithEncryption(Dictionary<string, string> data, SerializableDatabaseConfiguration<T> config) {
        using var aes = new CronusAesProvider(config.EncryptionKey!);
        using var encrypter = aes.GetEncrypter();
        using var fileStream = new FileStream(config.Path, FileMode.OpenOrCreate);
        using var cryptoStream = new CryptoStream(fileStream, encrypter, CryptoStreamMode.Write);
        using var streamWriter = new StreamWriter(cryptoStream);
        var json = JsonSerializer.Serialize(data, JsonContexts.Default.DictionaryStringString);
        await streamWriter.WriteAsync(json);
    }

    internal virtual async Task SerializeWithoutEncryption(Dictionary<string, string> data, SerializableDatabaseConfiguration<T> config) {
        using var stream = new FileStream(config.Path, FileMode.Create);
        await JsonSerializer.SerializeAsync(stream, data, JsonContexts.Default.DictionaryStringString);
    }
}
