namespace CronusDb;

/// <summary>
/// Configuration for generic databases
/// </summary>
/// <typeparam name="TValue">The type for the values</typeparam>
/// <typeparam name="TSerialized">The type for the serialized values</typeparam>
public record GenericDatabaseConfiguration<TValue, TSerialized> : DatabaseConfiguration {
    /// <summary>
    /// Serialization function for <typeparamref name="TValue"/> into <typeparamref name="TSerialized"/>.
    /// </summary>
    public required Func<TValue, TSerialized> ToTSerialized { get; init; }

    /// <summary>
    /// Deserialization function from <typeparamref name="TSerialized"/> into <typeparamref name="TValue"/>.
    /// </summary>
    public required Func<TSerialized, TValue> ToTValue { get; init; }
}
