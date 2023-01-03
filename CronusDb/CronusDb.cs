using System.Security.Cryptography;
using System.Text.Json;

namespace CronusDb;

/// <summary>
/// Cronus database
/// </summary>
/// <typeparam name="T">The type of the values in the KeyValuePairs</typeparam>
public sealed class CronusDb<T> {
    private readonly Dictionary<string, T> _data;
    private readonly CronusDbConfiguration<T>? _config;
    private readonly bool _isMemoryOnly;

    /// <summary>
    /// Triggered when an item is upserted.
    /// </summary>
    /// <remarks>
    /// You can use this to hook cache invalidation with <see cref="RemoveAny(Func{T, bool})"./>
    /// </remarks>
    public event EventHandler? ItemUpserted;

    public void OnItemUpserted(EventArgs e) {
        ItemUpserted?.Invoke(this, e);
    }

    /// <summary>
    /// Triggered when an item is removed.
    /// </summary>
    /// <remarks>
    /// You can use this to hook cache invalidation with <see cref="RemoveAny(Func{T, bool})"./>
    /// </remarks>
    public event EventHandler? ItemRemoved;

    public void OnItemRemoved(EventArgs e) {
        ItemRemoved?.Invoke(this, e);
    }

    // This constructor is used for a serializable instance
    internal CronusDb(Dictionary<string, T> data, CronusDbConfiguration<T> config) {
        _data = data;
        _config = config;
        _isMemoryOnly = false;
    }

    // This constructor is used for an In-Memory only instance
    internal CronusDb(Dictionary<string, T> data) {
        _data = data;
        _config = null;
        _isMemoryOnly = true;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise the default for <typeparamref name="T"/>.
    /// </summary>
    /// <param name="key"></param>
    public T? Get(string key) => _data.TryGetValue(key, out var value) ? value : default;

    /// <summary>
    /// Inserts or updates
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Upsert(string key, T value) {
        _data[key] = value;
        OnItemUpserted(EventArgs.Empty);
    }

    /// <summary>
    /// Checks whether the database contains the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    public bool ContainsKey(string key) => _data.ContainsKey(key);

    /// <summary>
    /// Removes a value by <paramref name="key"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns>True if successful, false if database did not contain <paramref name="key"/>.</returns>
    public bool Remove(string key) {
        var output = _data.Remove(key);
        if (output) {
            OnItemRemoved(EventArgs.Empty);
        }
        return output;
    }

    /// <summary>
    /// Loops through all database entries and removes any for which the <paramref name="selector"/> returns true.
    /// </summary>
    /// <param name="selector"></param>
    /// <remarks>
    /// This is the main way to perform cache invalidation.
    /// </remarks>
    public void RemoveAny(Func<T, bool> selector) {
        foreach (var (k, v) in _data) {
            if (selector.Invoke(v)) {
                Remove(k);
            }
        }
    }

    /// <summary>
    /// Serializes the database to the path in <see cref="CronusDbConfiguration{T}"/>.
    /// </summary>
    /// <remarks>
    /// If the instance created was In-Memory only, this method will not do anything.
    /// </remarks>
    public async Task Serialize() {
        if (_isMemoryOnly) {
            return;
        }

        var output = new Dictionary<string, string>();

        foreach (var (k, v) in _data) {
            output.Add(k, _config!.Serializer(v));
        }

        if (string.IsNullOrWhiteSpace(_config!.EncryptionKey)) {
            await SerializeWithoutEncryption(output);
            return;
        }
        await SerializeWithEncryption(output);
    }

    private async Task SerializeWithEncryption(Dictionary<string, string> data) {
        using var aes = new CronusAesProvider(_config!.EncryptionKey!);
        using var encrypter = aes.GetEncrypter();
        using var fileStream = new FileStream(_config!.Path, FileMode.OpenOrCreate);
        using var cryptoStream = new CryptoStream(fileStream, encrypter, CryptoStreamMode.Write);
        using var streamWriter = new StreamWriter(cryptoStream);
        var json = JsonSerializer.Serialize(data, JsonContexts.Default.DictionaryStringString);
        await streamWriter.WriteAsync(json);
    }

    private async Task SerializeWithoutEncryption(Dictionary<string, string> data) {
        using var stream = new FileStream(_config!.Path, FileMode.Create);
        await JsonSerializer.SerializeAsync(stream, data, JsonContexts.Default.DictionaryStringString);
    }

    ///// <summary>
    ///// Returns a new In-Memory only instance of a database.
    ///// </summary>
    //public static Task<CronusDb<T>> Create() {
    //    return Task.FromResult(new CronusDb<T>(new()));
    //}

    /// <summary>
    /// Returns a new serializable instance of a database.
    /// </summary>
    /// <param name="config"></param>
    /// <remarks>
    /// The file will only be created after using the <see cref="Serialize"/> method.
    /// </remarks>
    public static async Task<CronusDb<T>> Create(CronusDbConfiguration<T>? config = null) {
        if (config is null) {
            return new CronusDb<T>(new());
        }

        if (!File.Exists(config.Path)) {
            return new CronusDb<T>(new(), config);
        }

        var dict = string.IsNullOrWhiteSpace(config.EncryptionKey) ?
            await DeserializeWithoutEncyption(config)
            : await DeserializeWithEncyption(config);

        if (dict is null) {
            return new CronusDb<T>(new(), config);
        }

        var output = new Dictionary<string, T>();

        foreach (var (k, v) in dict) {
            output.Add(k, config.Deserializer(v));
        }

        return new CronusDb<T>(output, config);
    }

    private static async Task<Dictionary<string, string>?> DeserializeWithEncyption(CronusDbConfiguration<T> config) {
        using var aes = new CronusAesProvider(config.EncryptionKey!);
        using var decrypter = aes.GetDecrypter();
        using var fileStream = new FileStream(config.Path, FileMode.Open);
        using var cryptoStream = new CryptoStream(fileStream, decrypter!, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);
        var json = await streamReader.ReadToEndAsync();
        return JsonSerializer.Deserialize(json, JsonContexts.Default.DictionaryStringString);
    }

    private static async Task<Dictionary<string, string>?> DeserializeWithoutEncyption(CronusDbConfiguration<T> config) {
        using var stream = new FileStream(config!.Path, FileMode.Open);
        return await JsonSerializer.DeserializeAsync(stream, JsonContexts.Default.DictionaryStringString);
    }
}
