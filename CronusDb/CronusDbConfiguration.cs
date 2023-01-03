namespace CronusDb;

/// <summary>
/// Cronus database configuration
/// </summary>
/// <typeparam name="T">The type of the values in the KeyValuePairs</typeparam>
public record CronusDbConfiguration<T> {
    /// <summary>
    /// The path to the database file, including extension.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Encryption key for encrypting the database file with AES256.
    /// </summary>
    /// <remarks>
    /// <para>Leave as null if encryption is not required.</para>
    /// <para>Only effects serialization and deserialization</para>
    /// </remarks>
    public string? EncryptionKey { get; init; }

    /// <summary>
    /// Serialization function for <typeparamref name="T"/> into string.
    /// </summary>
    public required Func<T, string> Serializer { get; init; }

    /// <summary>
    /// Deserialization function for string into <typeparamref name="T"/>.
    /// </summary>
    public required Func<string, T> Deserializer { get; init; }
}
