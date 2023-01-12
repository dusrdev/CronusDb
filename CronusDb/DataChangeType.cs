namespace CronusDb;

/// <summary>
/// The type of changed that occurred on a key
/// </summary>
public enum DataChangeType {
    /// <summary>
    /// A new key was inserted
    /// </summary>
    Insert = 0,
    /// <summary>
    /// An existing key was updated
    /// </summary>
    Update = 1,
    /// <summary>
    /// A key was removed
    /// </summary>
    Remove = 2
}
