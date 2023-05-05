namespace CronusDb;

/// <summary>
/// Entry point for database creation
/// </summary>
public static class Cronus {
    /// <summary>
    /// Creates and returns a new instance of a database
    /// </summary>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This database supports optional per key encryption, and data is stored as byte[] so you can use different types of values as long as you serialize them to byte[]
    /// </remarks>
    public static Database CreateDatabase(DatabaseConfiguration configuration) {
        if (!File.Exists(configuration.Path)) {
            return new Database(
                new(configuration.Options.GetComparer()),
                configuration);
        }

        var dict = configuration.Path.Deserialize<byte[]>(
            configuration.EncryptionKey,
            configuration.Options);

        return new Database(dict, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of a generic value database
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// Similar to <see cref="Database"/> but converters allow adding un-serialized values thus vastly improving performance of CRUD operations, also more options are supported, such as <see cref="Database{TValue}.RemoveAny(Func{TValue, bool})"/> and more.
    /// </remarks>
    public static Database<TValue> CreateDatabase<TValue>(DatabaseConfiguration<TValue> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new Database<TValue>(
                new(configuration.Options.GetComparer()),
                configuration);
        }

        var dict = configuration.Path.Deserialize<byte[]>(
            configuration.EncryptionKey,
            configuration.Options);

        if (dict is null) {
            return new Database<TValue>(
                new(configuration.Options.GetComparer()),
                configuration);
        }

        var output = dict.Convert(configuration.ToTValue);

        return new Database<TValue>(output, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of a database
    /// </summary>
    /// <param name="configuration"></param>
    /// <remarks>
    /// This database supports optional per key encryption, and data is stored as byte[] so you can use different types of values as long as you serialize them to byte[]
    /// </remarks>
    public static async ValueTask<Database> CreateDatabaseAsync(DatabaseConfiguration configuration) {
        if (!File.Exists(configuration.Path)) {
            return new Database(
                new(configuration.Options.GetComparer()),
                configuration);
        }

        var dict = await configuration.Path.DeserializeAsync<byte[]>(
            configuration.EncryptionKey,
            configuration.Options);

        return new Database(dict, configuration);
    }

    /// <summary>
    /// Creates and returns a new instance of a generic value database
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="configuration"></param>
    /// <remarks>
    /// Similar to <see cref="Database"/> but converters allow adding un-serialized values thus vastly improving performance of CRUD operations, also more options are supported, such as <see cref="Database{TValue}.RemoveAny(Func{TValue, bool})"/> and more.
    /// </remarks>
    public static async ValueTask<Database<TValue>> CreateDatabaseAsync<TValue>(DatabaseConfiguration<TValue> configuration) {
        if (!File.Exists(configuration.Path)) {
            return new Database<TValue>(
                new(configuration.Options.GetComparer()),
                configuration);
        }

        var dict = await configuration.Path.DeserializeAsync<byte[]>(
            configuration.EncryptionKey,
            configuration.Options);

        if (dict is null) {
            return new Database<TValue>(
                new(configuration.Options.GetComparer()),
                configuration);
        }

        var output = dict.Convert(configuration.ToTValue);

        return new Database<TValue>(output, configuration);
    }
}
