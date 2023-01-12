using System.Collections.Concurrent;
using System.Text.Json;

namespace CronusDb;

/// <summary>
/// Main entry point for database creation
/// </summary>
public static class Cronus {
    /// <summary>
    /// Creates and returns a new instance of a serializable generic database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with fast crud operations and better invalidation using <see cref="GenericDatabase{T}.RemoveAny(Func{T, bool})"/>
    /// </remarks>
    public static CronusGenericDatabase<T> CreateGenericDatabase<T>(GenericDatabaseConfiguration<T> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new CronusGenericDatabase<T>(new(), configuration);
        }

        var dict = string.IsNullOrWhiteSpace(configuration.EncryptionKey) ?
            DeserializeWithoutEncyption(configuration.Path)
            : DeserializeWithEncyption(configuration.Path, configuration.EncryptionKey);

        if (dict is null) {
            return new CronusGenericDatabase<T>(new(), configuration);
        }

        var output = new ConcurrentDictionary<string, T>();

        foreach (var (k, v) in dict) {
            _ = output.TryAdd(k, configuration.FromStringConverter(v));
        }

        return new CronusGenericDatabase<T>(output, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of a serializable generic database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with fast crud operations and better invalidation using <see cref="GenericDatabase{T}.RemoveAny(Func{T, bool})"/>
    /// </remarks>
    public static async Task<CronusGenericDatabase<T>> CreateGenericDatabaseAsync<T>(GenericDatabaseConfiguration<T> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new CronusGenericDatabase<T>(new(), configuration);
        }

        var dict = string.IsNullOrWhiteSpace(configuration.EncryptionKey) ?
            await DeserializeWithoutEncyptionAsync(configuration.Path)
            : await DeserializeWithEncyptionAsync(configuration.Path, configuration.EncryptionKey);

        if (dict is null) {
            return new CronusGenericDatabase<T>(new(), configuration);
        }

        var output = new ConcurrentDictionary<string, T>();

        foreach (var (k, v) in dict) {
            _ = output.TryAdd(k, configuration.FromStringConverter(v));
        }

        return new CronusGenericDatabase<T>(output, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of an Polymorphic database
    /// </summary>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with optional per key and global encryption, and without value serializers so you could use different ones per value, enables values of different types.
    /// </remarks>
    public static CronusDatabase CreateDatabase(DatabaseConfiguration configuration) {
        if (!File.Exists(configuration.Path)) {
            return new CronusDatabase(new(), configuration);
        }

        var dict = string.IsNullOrWhiteSpace(configuration.EncryptionKey) ?
            DeserializeWithoutEncyption(configuration.Path)
            : DeserializeWithEncyption(configuration.Path, configuration.EncryptionKey);

        if (dict is null) {
            return new CronusDatabase(new(), configuration);
        }

        return new CronusDatabase(new(dict), configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of an Polymorphic database
    /// </summary>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with optional per key and global encryption, and without value serializers so you could use different ones per value, enables values of different types.
    /// </remarks>
    public static async ValueTask<CronusDatabase> CreateDatabaseAsync(DatabaseConfiguration configuration) {
        if (!File.Exists(configuration.Path)) {
            return new CronusDatabase(new(), configuration);
        }

        var dict = string.IsNullOrWhiteSpace(configuration.EncryptionKey) ?
            await DeserializeWithoutEncyptionAsync(configuration.Path)
            : await DeserializeWithEncyptionAsync(configuration.Path, configuration.EncryptionKey);

        if (dict is null) {
            return new CronusDatabase(new(), configuration);
        }

        return new CronusDatabase(new(dict), configuration);
    }

    private static IDictionary<string, string>? DeserializeWithEncyption(string path, string encryptionKey) {
        var content = File.ReadAllText(path);
        return DeserializeDict(content, path, encryptionKey);
    }

    private static async ValueTask<IDictionary<string, string>?> DeserializeWithEncyptionAsync(string path, string encryptionKey, CancellationToken token = default) {
        var content = await File.ReadAllTextAsync(path, token);
        return DeserializeDict(content, path, encryptionKey);
    }

    private static IDictionary<string, string>? DeserializeWithoutEncyption(string path) {
        var content = File.ReadAllText(path);
        return DeserializeDict(content, path);
    }

    private static async ValueTask<IDictionary<string, string>?> DeserializeWithoutEncyptionAsync(string path, CancellationToken token = default) {
        var content = await File.ReadAllTextAsync(path, token);
        return DeserializeDict(content, path);
    }

    private static IDictionary<string, string>? DeserializeDict(string content, string path, string? encryptionKey = null) {
        try {
            if (string.IsNullOrWhiteSpace(content)) {
                return default;
            }
            if (!string.IsNullOrWhiteSpace(encryptionKey)) {
                content = content.Decrypt(encryptionKey);
            }
            return JsonSerializer.Deserialize(content, JsonContexts.Default.IDictionaryStringString);
        } catch {
            throw new InvalidDataException($"Could not deserialize the database from <{path}>");
        }
    }
}
