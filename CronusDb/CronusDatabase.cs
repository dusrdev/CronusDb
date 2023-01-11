using System.Collections.Concurrent;
using System.Text.Json;

namespace CronusDb;

/// <summary>
/// Main entry point for database creation
/// </summary>
public static class CronusDatabase {
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
            DeserializeWithoutEncyption(configuration.Path)
            : DeserializeWithEncyption(configuration.Path, configuration.EncryptionKey);

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
            await DeserializeWithoutEncyptionAsync(configuration.Path)
            : await DeserializeWithEncyptionAsync(configuration.Path, configuration.EncryptionKey);

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
    /// Creates and returns a new instance of an Polymorphic database
    /// </summary>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with that enables per key and global encryption, and removes value serializers so you could use different ones per value, enabled multi-type values.
    /// </remarks>
    public static async ValueTask<PolymorphicDatabase> CreatePolymorphicDatabaseAsync(PolymorphicConfiguration configuration) {
        if (!File.Exists(configuration.Path)) {
            return new PolymorphicDatabase(new(), configuration);
        }

        var dict = string.IsNullOrWhiteSpace(configuration.EncryptionKey) ?
            await DeserializeWithoutEncyptionAsync(configuration.Path)
            : await DeserializeWithEncyptionAsync(configuration.Path, configuration.EncryptionKey);

        if (dict is null) {
            return new PolymorphicDatabase(new(), configuration);
        }

        return new PolymorphicDatabase(new(dict), configuration);
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
            DeserializeWithoutEncyption(configuration.Path)
            : DeserializeWithEncyption(configuration.Path, configuration.EncryptionKey);

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
            await DeserializeWithoutEncyptionAsync(configuration.Path)
            : await DeserializeWithEncyptionAsync(configuration.Path, configuration.EncryptionKey);

        if (dict is null) {
            return new SerializableDatabase<T>(new(), configuration);
        }

        return new SerializableDatabase<T>(new(dict), configuration);
    }

    private static IDictionary<string, string>? DeserializeWithEncyption(string path, string encryptionKey) {
        try {
            var content = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(content)) {
                return default;
            }
            var decrypted = content.Decrypt(encryptionKey!);
            return JsonSerializer.Deserialize(decrypted, JsonContexts.Default.IDictionaryStringString);
        } catch {
            throw new InvalidDataException($"Could not deserialize the database from <{path}>");
        }
    }

    private static async ValueTask<IDictionary<string, string>?> DeserializeWithEncyptionAsync(string path, string encryptionKey, CancellationToken token = default) {
        try {
            var content = await File.ReadAllTextAsync(path, token);
            if (string.IsNullOrWhiteSpace(content)) {
                return default;
            }
            var decrypted = content.Decrypt(encryptionKey!);
            return JsonSerializer.Deserialize(decrypted, JsonContexts.Default.IDictionaryStringString);
        } catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException) {
            throw;
        } catch {
            throw new InvalidDataException($"Could not deserialize the database from <{path}>");
        }
    }

    private static IDictionary<string, string>? DeserializeWithoutEncyption(string path) {
        try {
            var content = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(content)) {
                return default;
            }
            return JsonSerializer.Deserialize(content, JsonContexts.Default.IDictionaryStringString);
        } catch {
            throw new InvalidDataException($"Could not deserialize the database from <{path}>");
        }
    }

    private static async ValueTask<IDictionary<string, string>?> DeserializeWithoutEncyptionAsync(string path, CancellationToken token = default) {
        try {
            if (!File.Exists(path)) {
                return default;
            }
            var content = await File.ReadAllTextAsync(path, token);
            if (string.IsNullOrWhiteSpace(content)) {
                return default;
            }
            return JsonSerializer.Deserialize(content, JsonContexts.Default.IDictionaryStringString);
        } catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException) {
            throw;
        } catch {
            throw new InvalidDataException($"Could not deserialize the database from <{path}>");
        }
    }
}
