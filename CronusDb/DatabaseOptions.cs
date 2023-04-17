namespace CronusDb;

/// <summary>
/// Database options
/// </summary>
public enum DatabaseOptions {
    /// <summary>
    /// Serialize database after upsert or delete.
    /// </summary>
    /// <remarks>
    /// If <see cref="Database{TValue}.RemoveAny(Func{TValue, bool})"/> is called serialization will only be done once, after execution is finished.
    /// </remarks>
    SerializeOnUpdate = 1 << 0,

    /// <summary>
    /// Trigger the OnDataChanged event on upsert and delete.
    /// </summary>
    /// <remarks>
    /// Is disabled by default to improve performance
    /// </remarks>
    TriggerUpdateEvents = 1 << 1,
}
