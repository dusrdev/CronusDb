namespace CronusDb;

/// <summary>
/// Configuration for serializable databases
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TSerialized"></typeparam>
public record GenericDatabaseConfiguration<TValue, TSerialized> {
    /// <summary>
    /// The path to the database file, including extension.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Automatically trigger serialization on Insert, Update and Delete.
    /// </summary>
    public required bool SerializeOnUpdate { get; init; }

    /// <summary>
    /// Encryption key for encrypting the database file with AES256.
    /// </summary>
    /// <remarks>
    /// <para>Leave as null if encryption is not required.</para>
    /// <para>Only effects serialization and deserialization</para>
    /// </remarks>
    public string? EncryptionKey { get; init; }

    /// <summary>
    /// Serialization function for <typeparamref name="TValue"/> into <typeparamref name="TSerialized"/>.
    /// </summary>
    public required Func<TValue, TSerialized> ToTSerialized { get; init; }

    /// <summary>
    /// Deserialization function from <typeparamref name="TSerialized"/> into <typeparamref name="TValue"/>.
    /// </summary>
    public required Func<TSerialized, TValue> ToTValue { get; init; }
}
