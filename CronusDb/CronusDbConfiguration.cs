namespace CronusDb;

public record CronusDbConfiguration<T> {
    public required string Path { get; init; }
    public string? EncryptionKey { get; init; }
    public required Func<T, string> Serializer { get; init; }
    public required Func<string, T> Deserializer { get; init; }
}
