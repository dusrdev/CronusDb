namespace CronusDb;

/// <summary>
/// Configuration for the polymorphic database
/// </summary>
public record PolymorphicConfiguration {
    /// <summary>
    /// The path to which the database file will be saved.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// General encryption key, the entire file will be encrypted with this.
    /// </summary>
    public string? EncryptionKey { get; init; }
}
