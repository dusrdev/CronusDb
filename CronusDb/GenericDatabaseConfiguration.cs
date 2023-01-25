namespace CronusDb;

/// <summary>
/// Configuration for generic databases
/// </summary>
/// <typeparam name="TValue">The type for the values</typeparam>
public record DatabaseConfiguration<TValue> : DatabaseConfiguration {
    /// <summary>
    /// Serialization function for <typeparamref name="TValue"/> into byte[].
    /// </summary>
    public required Func<TValue, byte[]> ToTSerialized { get; init; }

    /// <summary>
    /// Deserialization function from byte[] into <typeparamref name="TValue"/>.
    /// </summary>
    public required Func<byte[], TValue> ToTValue { get; init; }
}
