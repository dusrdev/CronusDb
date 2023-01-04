namespace CronusDb;

public abstract class CronusDatabase<T> {
    /// <summary>
    /// Triggered when an item is upserted.
    /// </summary>
    /// <remarks>
    /// You can use this to hook cache invalidation with <see cref="RemoveAny(Func{T, bool})"/>.
    /// </remarks>
    public event EventHandler? ItemUpserted;

    internal virtual void OnItemUpserted(EventArgs e) {
        ItemUpserted?.Invoke(this, e);
    }

    /// <summary>
    /// Triggered when an item is removed.
    /// </summary>
    /// <remarks>
    /// You can use this to hook cache invalidation with <see cref="RemoveAny(Func{T, bool})"/>.
    /// </remarks>
    public event EventHandler? ItemRemoved;

    internal virtual void OnItemRemoved(EventArgs e) {
        ItemRemoved?.Invoke(this, e);
    }

    public abstract bool ContainsKey(string key);

    public abstract T? Get(string key);

    public abstract void Upsert(string key, T value);

    public abstract bool Remove(string key);

    public abstract void RemoveAny(Func<T, bool> selector);

    public abstract Task Serialize();

    internal abstract Task SerializeWithEncryption(Dictionary<string, string> data);

    internal abstract Task SerializeWithoutEncryption(Dictionary<string, string> data);
}
