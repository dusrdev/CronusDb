using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CronusDb;

/// <summary>
/// Base type for the database
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class GenericDatabase<T> {
    /// <summary>
    /// Triggered when there is a change in the database.
    /// </summary>
    public event EventHandler<DataChangedEventArgs>? DataChanged;

    internal virtual void OnDataChanged(DataChangedEventArgs e) {
        DataChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Checked whether the inner dictionary contains the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    public abstract bool ContainsKey(string key);

    /// <summary>
    /// Returns the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    public abstract T? Get(string key);

    /// <summary>
    /// Updates or inserts a new <paramref name="value"/> with the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public abstract void Upsert(string key,[DisallowNull] T value);

    /// <summary>
    /// An indexer option for <see cref="Get(string)"/> and <see cref="Upsert(string, T)"/>.
    /// </summary>
    /// <param name="index"></param>
    public virtual T? this[string index] {
        get => Get(index);
        set => Upsert(index, value!);
    }

    /// <summary>
    /// Removes the <paramref name="key"/> and its value from the inner dictionary.
    /// </summary>
    /// <param name="key"></param>
    public abstract bool Remove(string key);

    /// <summary>
    /// Removes all the keys and values in the dictionary for which the value passes the <paramref name="selector"/>.
    /// </summary>
    /// <param name="selector"></param>
    public abstract void RemoveAny(Func<T, bool> selector);

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public abstract Task SerializeAsync();

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public abstract void Serialize();

    internal virtual void SerializeWithEncryption(IDictionary<string, string> data, GenericDatabaseConfiguration<T> config) {
        var json = JsonSerializer.Serialize(data, JsonContexts.Default.IDictionaryStringString);
        var encrypted = json.Encrypt(config.EncryptionKey!);
        File.WriteAllText(config.Path, encrypted);
    }

    internal virtual async Task SerializeWithEncryptionAsync(IDictionary<string, string> data, GenericDatabaseConfiguration<T> config) {
        var json = JsonSerializer.Serialize(data, JsonContexts.Default.IDictionaryStringString);
        var encrypted = json.Encrypt(config.EncryptionKey!);
        await File.WriteAllTextAsync(config.Path, encrypted);
    }

    internal virtual void SerializeWithoutEncryption(IDictionary<string, string> data, GenericDatabaseConfiguration<T> config) {
        var json = JsonSerializer.Serialize(data, JsonContexts.Default.IDictionaryStringString);
        File.WriteAllText(config.Path, json);
    }

    internal virtual async Task SerializeWithoutEncryptionAsync(IDictionary<string, string> data, GenericDatabaseConfiguration<T> config) {
        var json = JsonSerializer.Serialize(data, JsonContexts.Default.IDictionaryStringString);
        await File.WriteAllTextAsync(config.Path, json);
    }
}
