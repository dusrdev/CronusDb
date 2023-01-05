using System.Collections.Concurrent;

namespace CronusDb;

/// <summary>
/// In memory database, no disk operation, maximum CRUD performance.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class InMemoryDatabase<T> : CronusDatabase<T> {
    private readonly ConcurrentDictionary<string, T> _data;

    // This constructor is used for an In-Memory only instance
    internal InMemoryDatabase(ConcurrentDictionary<string, T> data) {
        _data = data;
    }

    /// <summary>
    /// Returns the value if it exists, otherwise the default for <typeparamref name="T"/>.
    /// </summary>
    /// <param name="key"></param>
    public override T? Get(string key) => _data.TryGetValue(key, out var value) ? value : default;

    /// <summary>
    /// Inserts or updates
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public override void Upsert(string key, T value) {
        _data[key] = value;
        OnItemUpserted(EventArgs.Empty);
    }

    /// <summary>
    /// Checks whether the database contains the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    public override bool ContainsKey(string key) => _data.ContainsKey(key);

    /// <summary>
    /// Removes a value by <paramref name="key"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns>True if successful, false if database did not contain <paramref name="key"/>.</returns>
    public override bool Remove(string key) {
        var output = _data.Remove(key, out _);
        if (output) {
            OnItemRemoved(EventArgs.Empty);
        }
        return output;
    }

    /// <summary>
    /// Loops through all database entries and removes any for which the <paramref name="selector"/> returns true.
    /// </summary>
    /// <param name="selector"></param>
    /// <remarks>
    /// This is the main way to perform cache invalidation.
    /// </remarks>
    public override void RemoveAny(Func<T, bool> selector) {
        foreach (var (k, v) in _data) {
            if (selector.Invoke(v)) {
                Remove(k);
            }
        }
    }

    /// <summary>
    /// As this is an In-Memory only database, it does nothing.
    /// </summary>
    public override Task SerializeAsync() {
        return Task.CompletedTask;
    }
}
