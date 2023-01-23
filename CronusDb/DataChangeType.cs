namespace CronusDb;

/// <summary>
/// The type of changed that occurred on a key
/// </summary>
public enum DataChangeType {
    /// <summary>
    /// A key was inserted or updated
    /// </summary>
    Upsert = 0,
    /// <summary>
    /// A key was removed
    /// </summary>
    Remove = 1
}
