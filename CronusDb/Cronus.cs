namespace CronusDb;

/// <summary>
/// Main entry point for database creation
/// </summary>
public static class Cronus {
    /// <summary>
    /// Creates and returns a new instance of a binary value database
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// Similar to <see cref="CronusGenericDatabase{TValue}"/> but even more efficient serialization
    /// </remarks>
    public static CronusBinaryDatabase<TValue> CreateBinaryDatabase<TValue>(GenericDatabaseConfiguration<TValue, byte[]> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new CronusBinaryDatabase<TValue>(new(), configuration);
        }

        var dict = configuration.Path.Deserialize<byte[]>(configuration.EncryptionKey);

        if (dict is null) {
            return new CronusBinaryDatabase<TValue>(new(), configuration);
        }

        var output = dict.Convert(configuration.ToTValue);

        return new CronusBinaryDatabase<TValue>(output, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of a binary value database
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// Similar to <see cref="CronusGenericDatabase{TValue}"/> but even more efficient serialization
    /// </remarks>
    public static async ValueTask<CronusBinaryDatabase<TValue>> CreateBinaryDatabaseAsync<TValue>(GenericDatabaseConfiguration<TValue, byte[]> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new CronusBinaryDatabase<TValue>(new(), configuration);
        }

        var dict = await configuration.Path.DeserializeAsync<byte[]>(configuration.EncryptionKey);

        if (dict is null) {
            return new CronusBinaryDatabase<TValue>(new(), configuration);
        }

        var output = dict.Convert(configuration.ToTValue);

        return new CronusBinaryDatabase<TValue>(output, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of a string value database
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

        var dict = configuration.Path.Deserialize<string>(configuration.EncryptionKey);

        if (dict is null) {
            return new CronusGenericDatabase<TValue>(new(), configuration);
        }

        var output = dict.Convert(configuration.ToTValue);

        return new CronusGenericDatabase<TValue>(output, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of a string value database
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

        var dict = await configuration.Path.DeserializeAsync<string>(configuration.EncryptionKey);

        if (dict is null) {
            return new CronusGenericDatabase<TValue>(new(), configuration);
        }

        var output = dict.Convert(configuration.ToTValue);

        return new CronusGenericDatabase<TValue>(output, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of a database
    /// </summary>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with optional per key and global encryption, and without value serializers so you could use different ones per value, enables values of different types.
    /// </remarks>
    public static CronusDatabase CreateDatabase(DatabaseConfiguration configuration) {
        if (!File.Exists(configuration.Path)) {
            return new CronusDatabase(new(), configuration);
        }

        var dict = configuration.Path.Deserialize<string>(configuration.EncryptionKey);

        return new CronusDatabase(dict, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of a database
    /// </summary>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with optional per key and global encryption, and without value serializers so you could use different ones per value, enables values of different types.
    /// </remarks>
    public static async ValueTask<CronusDatabase> CreateDatabaseAsync(DatabaseConfiguration configuration) {
        if (!File.Exists(configuration.Path)) {
            return new CronusDatabase(new(), configuration);
        }

        var dict = await configuration.Path.DeserializeAsync<string>(configuration.EncryptionKey);

        return new CronusDatabase(dict, configuration);
    }
}
