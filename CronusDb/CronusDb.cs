using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;

namespace CronusDb;

/// <summary>
/// Main entry point for database creation
/// </summary>
public static class CronusDb {
    /// <summary>
    /// Creates and returns a new instance of an In-Memory only database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static InMemoryDatabase<T> CreateInMemoryDatabase<T>() {
        return new InMemoryDatabase<T>(new());
    }

    /// <summary>
    /// Creates and returns a new instance of an In-Memory only database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static Task<InMemoryDatabase<T>> CreateInMemoryDatabaseAsync<T>() {
        return Task.FromResult(new InMemoryDatabase<T>(new()));
    }

    /// <summary>
    /// Creates and returns a new instance of an Serializable in-Memory database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with In-Memory only CRUD performance, but serialization is slower and less memory efficient than <see cref="SerializableDatabase{T}"/>
    /// </remarks>
    public static SerializableInMemoryDatabase<T> CreateSerializableInMemoryDatabase<T>(SerializableDatabaseConfiguration<T> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new SerializableInMemoryDatabase<T>(new(), configuration);
        }

        var dict = string.IsNullOrWhiteSpace(configuration.EncryptionKey) ?
            DeserializeWithoutEncyption(configuration)
            : DeserializeWithEncyption(configuration);

        if (dict is null) {
            return new SerializableInMemoryDatabase<T>(new(), configuration);
        }

        var output = new ConcurrentDictionary<string, T>();

        foreach (var (k, v) in dict) {
            _ = output.TryAdd(k, configuration.FromStringConverter(v));
        }

        return new SerializableInMemoryDatabase<T>(output, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of an Serializable in-Memory database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with In-Memory only CRUD performance, but serialization is slower and less memory efficient than <see cref="SerializableDatabase{T}"/>
    /// </remarks>
    public static async Task<SerializableInMemoryDatabase<T>> CreateSerializableInMemoryDatabaseAsync<T>(SerializableDatabaseConfiguration<T> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new SerializableInMemoryDatabase<T>(new(), configuration);
        }

        var dict = string.IsNullOrWhiteSpace(configuration.EncryptionKey) ?
            await DeserializeWithoutEncyptionAsync(configuration)
            : await DeserializeWithEncyptionAsync(configuration);

        if (dict is null) {
            return new SerializableInMemoryDatabase<T>(new(), configuration);
        }

        var output = new ConcurrentDictionary<string, T>();

        foreach (var (k, v) in dict) {
            _ = output.TryAdd(k, configuration.FromStringConverter(v));
        }

        return new SerializableInMemoryDatabase<T>(output, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of an Serializable database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with slightly slower than In-Memory only CRUD performance, but serialization is drastically faster and more efficient than <see cref="SerializableInMemoryDatabase{T}"/>
    /// </remarks>
    public static SerializableDatabase<T> CreateSerializableDatabase<T>(SerializableDatabaseConfiguration<T> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new SerializableDatabase<T>(new(), configuration);
        }

        var dict = string.IsNullOrWhiteSpace(configuration.EncryptionKey) ?
            DeserializeWithoutEncyption(configuration)
            : DeserializeWithEncyption(configuration);

        if (dict is null) {
            return new SerializableDatabase<T>(new(), configuration);
        }

        return new SerializableDatabase<T>(new(dict), configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of an Serializable database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with slightly slower than In-Memory only CRUD performance, but serialization is drastically faster and more efficient than <see cref="SerializableInMemoryDatabase{T}"/>
    /// </remarks>
    public static async Task<SerializableDatabase<T>> CreateSerializableDatabaseAsync<T>(SerializableDatabaseConfiguration<T> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new SerializableDatabase<T>(new(), configuration);
        }

        var dict = string.IsNullOrWhiteSpace(configuration.EncryptionKey) ?
            await DeserializeWithoutEncyptionAsync(configuration)
            : await DeserializeWithEncyptionAsync(configuration);

        if (dict is null) {
            return new SerializableDatabase<T>(new(), configuration);
        }

        return new SerializableDatabase<T>(new(dict), configuration);
    }

    private static IDictionary<string, string>? DeserializeWithEncyption<T>(SerializableDatabaseConfiguration<T> config) {
        try {
            using var aes = new CronusAesProvider(config.EncryptionKey!);
            using var decrypter = aes.GetDecrypter();
            using var fileStream = new FileStream(config.Path, FileMode.Open);
            using var cryptoStream = new CryptoStream(fileStream, decrypter!, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);
            var json = streamReader.ReadToEnd();
            return JsonSerializer.Deserialize(json, JsonContexts.Default.IDictionaryStringString);
        } catch {
            throw new InvalidDataException($"Could not deserialize the database from <{config.Path}>");
        }
    }

    private static async Task<IDictionary<string, string>?> DeserializeWithEncyptionAsync<T>(SerializableDatabaseConfiguration<T> config, CancellationToken token = default) {
        try {
            using var aes = new CronusAesProvider(config.EncryptionKey!);
            using var decrypter = aes.GetDecrypter();
            using var fileStream = new FileStream(config.Path, FileMode.Open);
            using var cryptoStream = new CryptoStream(fileStream, decrypter!, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);
            var json = await streamReader.ReadToEndAsync(token);
            return JsonSerializer.Deserialize(json, JsonContexts.Default.IDictionaryStringString);
        } catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException) {
            throw;
        } catch {
            throw new InvalidDataException($"Could not deserialize the database from <{config.Path}>");
        }
    }

    private static IDictionary<string, string>? DeserializeWithoutEncyption<T>(SerializableDatabaseConfiguration<T> config) {
        try {
            using var stream = new FileStream(config!.Path, FileMode.Open);
            return JsonSerializer.Deserialize(stream, JsonContexts.Default.IDictionaryStringString);
        } catch {
            throw new InvalidDataException($"Could not deserialize the database from <{config.Path}>");
        }
    }

    private static async ValueTask<IDictionary<string, string>?> DeserializeWithoutEncyptionAsync<T>(SerializableDatabaseConfiguration<T> config, CancellationToken token = default) {
        try {
            using var stream = new FileStream(config!.Path, FileMode.Open);
            return await JsonSerializer.DeserializeAsync(stream, JsonContexts.Default.IDictionaryStringString, token);
        } catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException) {
            throw;
        } catch {
            throw new InvalidDataException($"Could not deserialize the database from <{config.Path}>");
        }
    }
}
