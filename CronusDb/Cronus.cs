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
    /// Similar to <see cref="CronusGenericDatabase{TValue, TSerialized}"/> but even more efficient serialization
    /// </remarks>
    public static Database<TValue, byte[]> CreateBinaryDatabase<TValue>(GenericDatabaseConfiguration<TValue, byte[]> configuration)
        => CreateGenericDatabaseInternal(configuration);

    /// <summary>
    /// Creates and returns a new instance of a binary value database
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// Similar to <see cref="CronusGenericDatabase{TValue, TSerialized}"/> but even more efficient serialization
    /// </remarks>
    public static ValueTask<Database<TValue, byte[]>> CreateBinaryDatabaseAsync<TValue>(GenericDatabaseConfiguration<TValue, byte[]> configuration) => CreateGenericDatabaseAsyncInternal(configuration);

    /// <summary>
    /// Creates and returns a new instance of a string value database
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with fast crud operations and better invalidation using <see cref="CronusGenericDatabase{TValue, TSerialized}.RemoveAny(Func{TValue, bool})"/>
    /// </remarks>
    public static Database<TValue, string> CreateGenericDatabase<TValue>(GenericDatabaseConfiguration<TValue, string> configuration)
        => CreateGenericDatabaseInternal(configuration);

    /// <summary>
    /// Creates and returns a new instance of a string value database
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with fast crud operations and better invalidation using <see cref="CronusGenericDatabase{TValue, TSerialized}.RemoveAny(Func{TValue, bool})"/>
    /// </remarks>
    public static ValueTask<Database<TValue, string>> CreateGenericDatabaseAsync<TValue>(GenericDatabaseConfiguration<TValue, string> configuration) => CreateGenericDatabaseAsyncInternal(configuration);

    private static Database<TValue, TSerialized> CreateGenericDatabaseInternal<TValue, TSerialized>(GenericDatabaseConfiguration<TValue, TSerialized> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new CronusGenericDatabase<TValue, TSerialized>(new(), configuration);
        }

        var dict = configuration.Path.Deserialize<TSerialized>(configuration.EncryptionKey);

        if (dict is null) {
            return new CronusGenericDatabase<TValue, TSerialized>(new(), configuration);
        }

        var output = dict.Convert(configuration.ToTValue);

        return new CronusGenericDatabase<TValue, TSerialized>(output, configuration);
    }

    private static async ValueTask<Database<TValue, TSerialized>> CreateGenericDatabaseAsyncInternal<TValue, TSerialized>(GenericDatabaseConfiguration<TValue, TSerialized> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new CronusGenericDatabase<TValue, TSerialized>(new(), configuration);
        }

        var dict = await configuration.Path.DeserializeAsync<TSerialized>(configuration.EncryptionKey);

        if (dict is null) {
            return new CronusGenericDatabase<TValue, TSerialized>(new(), configuration);
        }

        var output = dict.Convert(configuration.ToTValue);

        return new CronusGenericDatabase<TValue, TSerialized>(output, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of a database
    /// </summary>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This is a serializable database with optional per key and global encryption, and without value serializers so you could use different ones per value, enables values of different types.
    /// </remarks>
    public static Database CreateDatabase(DatabaseConfiguration configuration) {
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
    public static async ValueTask<Database> CreateDatabaseAsync(DatabaseConfiguration configuration) {
        if (!File.Exists(configuration.Path)) {
            return new CronusDatabase(new(), configuration);
        }

        var dict = await configuration.Path.DeserializeAsync<string>(configuration.EncryptionKey);

        return new CronusDatabase(dict, configuration);
    }
}
