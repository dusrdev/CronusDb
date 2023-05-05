using System.Collections.Concurrent;

using MemoryPack;

namespace CronusDb;

internal static class Serializer {
    internal static void Serialize<TSerialized>(this ConcurrentDictionary<string, TSerialized> data, string path, string? encryptionKey) {
        var bin = MemoryPackSerializer.Serialize(data);
        File.WriteAllBytes(path, string.IsNullOrWhiteSpace(encryptionKey) ? bin : bin.Encrypt(encryptionKey));
    }

    internal static async Task SerializeAsync<TSerialized>(this ConcurrentDictionary<string, TSerialized> data, string path, string? encryptionKey) {
        var bin = MemoryPackSerializer.Serialize(data);
        await File.WriteAllBytesAsync(path, string.IsNullOrWhiteSpace(encryptionKey) ? bin : bin.Encrypt(encryptionKey)).ConfigureAwait(false);
    }

    internal static ConcurrentDictionary<string, TSerialized>? Deserialize<TSerialized>(
        this string path,
        string? encryptionKey,
        DatabaseOptions options = 0) {
        var bin = File.ReadAllBytes(path);
        return DeserializeDict<TSerialized>(bin, path, options, encryptionKey);
    }

    internal static async Task<ConcurrentDictionary<string, TSerialized>?> DeserializeAsync<TSerialized>(
        this string path,
        string? encryptionKey,
        DatabaseOptions options = 0,
        CancellationToken token = default) {
        var bin = await File.ReadAllBytesAsync(path, token).ConfigureAwait(false);
        return DeserializeDict<TSerialized>(bin, path, options, encryptionKey);
    }

    private static ConcurrentDictionary<string, TSerialized>? DeserializeDict<TSerialized>(
        byte[] bin,
        string path,
        DatabaseOptions options,
        string? encryptionKey = null) {
        try {
            if (bin.Length is 0) {
                return default;
            }
            var buffer = string.IsNullOrWhiteSpace(encryptionKey)
                ? bin
                : bin.Decrypt(encryptionKey!);
            var deserialized = MemoryPackSerializer.Deserialize<ConcurrentDictionary<string, TSerialized>>(buffer);
            //TODO: find way to incorporate comparer into deserialization
            // Workaround region
            // Get comparer just once
            var comparer = options.GetComparer();

            // If the deserialized dictionary is null, return a new one with the comparer
            // comparer is null if the options are default
            if (deserialized is null) {
                return new(comparer);
            }

            // Deserialized is not null and no comparer needed so return it
            if (comparer is null) {
                return deserialized;
            }

            // Copy the deserialized dictionary into a new one with the comparer
            return new(deserialized, comparer);
        } catch {
            throw new InvalidDataException($"Could not deserialize the database from <{path}>");
        }
    }

    internal static ConcurrentDictionary<string, TSerialized> Convert<TValue, TSerialized>(this ConcurrentDictionary<string, TValue> dict, Func<TValue, TSerialized> converter) {
        var newDict = new ConcurrentDictionary<string, TSerialized>(dict.Comparer);
        if (dict.IsEmpty) {
            return newDict;
        }
        foreach (var (k, v) in dict) {
            try {
                newDict[k] = converter(v);
            } catch {
                throw new InvalidOperationException($"Converting the value of key <{k}> failed.");
            }
        }
        return newDict;
    }
}
