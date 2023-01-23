using MemoryPack;

using System.Collections.Concurrent;

namespace CronusDb;

/// <summary>
/// Main entry point for database creation
/// </summary>
public static class Cronus {
    /// <summary>
    /// Creates and returns a new instance of a serializable binary database
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with fast crud operations and better invalidation using <see cref="CronusBinaryDatabase{TValue}.RemoveAny(Func{TValue, bool})"/>
    /// </remarks>
    public static CronusBinaryDatabase<TValue> CreateBinaryDatabase<TValue>(GenericDatabaseConfiguration<TValue, byte[]> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new CronusBinaryDatabase<TValue>(new(), configuration);
        }

        var dict = Deserialize<byte[]>(configuration.Path, configuration.EncryptionKey);

        if (dict is null) {
            return new CronusBinaryDatabase<TValue>(new(), configuration);
        }

        var output = dict.Convert(configuration.ToTValue);

        return new CronusBinaryDatabase<TValue>(output, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of a serializable binary database
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with fast crud operations and better invalidation using <see cref="CronusBinaryDatabase{TValue}.RemoveAny(Func{TValue, bool})"/>
    /// </remarks>
    public static async ValueTask<CronusBinaryDatabase<TValue>> CreateBinaryDatabaseAsync<TValue>(GenericDatabaseConfiguration<TValue, byte[]> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new CronusBinaryDatabase<TValue>(new(), configuration);
        }

        var dict = await DeserializeAsync<byte[]>(configuration.Path, configuration.EncryptionKey);

        if (dict is null) {
            return new CronusBinaryDatabase<TValue>(new(), configuration);
        }

        var output = dict.Convert(configuration.ToTValue);

        return new CronusBinaryDatabase<TValue>(output, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of a serializable generic database
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with fast crud operations and better invalidation using <see cref="CronusGenericDatabase{TValue}.RemoveAny(Func{TValue, bool})"/>
    /// </remarks>
    public static CronusGenericDatabase<TValue> CreateGenericDatabase<TValue>(GenericDatabaseConfiguration<TValue, string> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new CronusGenericDatabase<TValue>(new(), configuration);
        }

        var dict = Deserialize<string>(configuration.Path, configuration.EncryptionKey);

        if (dict is null) {
            return new CronusGenericDatabase<TValue>(new(), configuration);
        }

        var output = dict.Convert(configuration.ToTValue);

        return new CronusGenericDatabase<TValue>(output, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of a serializable generic database
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with fast crud operations and better invalidation using <see cref="CronusGenericDatabase{TValue}.RemoveAny(Func{TValue, bool})"/>
    /// </remarks>
    public static async ValueTask<CronusGenericDatabase<TValue>> CreateGenericDatabaseAsync<TValue>(GenericDatabaseConfiguration<TValue, string> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new CronusGenericDatabase<TValue>(new(), configuration);
        }

        var dict = await DeserializeAsync<string>(configuration.Path, configuration.EncryptionKey);

        if (dict is null) {
            return new CronusGenericDatabase<TValue>(new(), configuration);
        }

        var output = dict.Convert(configuration.ToTValue);

        return new CronusGenericDatabase<TValue>(output, configuration);
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

        var dict = Deserialize<string>(configuration.Path, configuration.EncryptionKey);

        return new CronusDatabase(dict, configuration);
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

        var dict = await DeserializeAsync<string>(configuration.Path, configuration.EncryptionKey);

        return new CronusDatabase(dict, configuration);
    }

    private static ConcurrentDictionary<string, TSerialized>? Deserialize<TSerialized>(string path, string? encryptionKey) {
        var bin = File.ReadAllBytes(path);
        return DeserializeDict<TSerialized>(bin, path, encryptionKey);
    }

    private static async Task<ConcurrentDictionary<string, TSerialized>?> DeserializeAsync<TSerialized>(string path, string? encryptionKey, CancellationToken token = default) {
        var bin = await File.ReadAllBytesAsync(path, token);
        return DeserializeDict<TSerialized>(bin, path, encryptionKey);
    }

    private static ConcurrentDictionary<string, TSerialized>? DeserializeDict<TSerialized>(ReadOnlySpan<byte> bin, string path, string? encryptionKey = null) {
        try {
            if (bin.Length is 0) {
                return default;
            }
            var buffer = string.IsNullOrWhiteSpace(encryptionKey)
                ? bin
                : bin.Decrypt(encryptionKey!);
            return MemoryPackSerializer.Deserialize<ConcurrentDictionary<string, TSerialized>>(buffer);
        } catch {
            throw new InvalidDataException($"Could not deserialize the database from <{path}>");
        }
    }
}
