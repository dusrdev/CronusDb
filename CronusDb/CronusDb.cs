using System.Text.Json;

namespace CronusDb;

public sealed class CronusDb<T> {
    private readonly Dictionary<string, T> _data;
    private readonly CronusDbConfiguration<T>? _config;
    private readonly bool _isMemoryOnly;

    internal CronusDb(Dictionary<string, T> data, CronusDbConfiguration<T>? config) {
        _data = data;
        _config = config;
        _isMemoryOnly = false;
    }

    internal CronusDb(Dictionary<string, T> data) {
        _data = data;
        _config = null;
        _isMemoryOnly = true;
    }

    public T? GetValue(string key) => _data.TryGetValue(key, out var value) ? value : default;

    public void SetValue(string key, T value) => _data[key] = value;

    public bool KeyExists(string key) => _data.ContainsKey(key);

    public bool Remove(string key) => _data.Remove(key);

    public void RemoveAny(Func<T, bool> selector) {
        foreach (var (k, v) in _data) {
            if (selector.Invoke(v)) {
                _data.Remove(k);
            }
        }
    }

    /// <summary>
    /// Serializes the database to the path in <see cref="DbConfiguration{T}"/>
    /// </summary>
    /// <remarks>
    /// If the instance created was In-Memory only, this method will not do anything.
    /// </remarks>
    public async Task Serialize() {
        if (_isMemoryOnly) {
            return;
        }

        var output = new Dictionary<string, string>();

        foreach (var (k, v) in _data) {
            output.Add(k, _config!.Serializer(v));
        }

        using var stream = new FileStream(_config!.Path, FileMode.Create);
        await JsonSerializer.SerializeAsync(stream, output, JsonContexts.Default.DictionaryStringString);
    }

    /// <summary>
    /// Returns a new In-Memory only instance of a database
    /// </summary>
    public static Task<CronusDb<T>> Create() {
        return Task.FromResult(new CronusDb<T>(new()));
    }

    /// <summary>
    /// Returns a new serializable In-Memory instance of a database
    /// </summary>
    /// <param name="config"></param>
    /// <remarks>
    /// The file will only be created after using the <see cref="Serialize"/> method.
    /// </remarks>
    public static async Task<CronusDb<T>> Create(CronusDbConfiguration<T> config) {
        if (!File.Exists(config.Path)) {
            return new CronusDb<T>(new(), config);
        }

        using var stream = new FileStream(config.Path, FileMode.Open);
        var dict = await JsonSerializer.DeserializeAsync(stream, JsonContexts.Default.DictionaryStringString);

        if (dict is null) {
            return new CronusDb<T>(new());
        }

        var output = new Dictionary<string, T>();

        foreach (var (k, v) in dict) {
            output.Add(k, config.Deserializer(v));
        }

        return new CronusDb<T>(output, config);
    }
}
