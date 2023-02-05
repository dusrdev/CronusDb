using MemoryPack;

using System.Collections.Concurrent;

namespace CronusDb;

internal static class Serializer {
    internal static void Serialize<TSerialized>(this ConcurrentDictionary<string, TSerialized> data, string path, string? encryptionKey) {
        var bin = MemoryPackSerializer.Serialize(data);
        File.WriteAllBytes(path, string.IsNullOrWhiteSpace(encryptionKey) ? bin : bin.Encrypt(encryptionKey));
    }

    internal static async Task SerializeAsync<TSerialized>(this ConcurrentDictionary<string, TSerialized> data, string path, string? encryptionKey) {
        var bin = MemoryPackSerializer.Serialize(data);
        await File.WriteAllBytesAsync(path, string.IsNullOrWhiteSpace(encryptionKey) ? bin : bin.Encrypt(encryptionKey));
    }

    internal static ConcurrentDictionary<string, TSerialized>? Deserialize<TSerialized>(this string path, string? encryptionKey) {
        var bin = File.ReadAllBytes(path);
        return DeserializeDict<TSerialized>(bin, path, encryptionKey);
    }

    internal static async Task<ConcurrentDictionary<string, TSerialized>?> DeserializeAsync<TSerialized>(this string path, string? encryptionKey, CancellationToken token = default) {
        var bin = await File.ReadAllBytesAsync(path, token);
        return DeserializeDict<TSerialized>(bin, path, encryptionKey);
    }

    private static ConcurrentDictionary<string, TSerialized>? DeserializeDict<TSerialized>(byte[] bin, string path, string? encryptionKey = null) {
        try {
            if (bin.Length is 0) {
                return default;
            }
            var buffer = string.IsNullOrWhiteSpace(encryptionKey)
                ? bin
                : bin.Decrypt(encryptionKey!);
            return MemoryPackSerializer.Deserialize<ConcurrentDictionary<string, TSerialized>>(buffer);
        } catch {
            throw new InvalidDataException($"Could not deserialize the database from <{path}>");
        }
    }

    internal static ConcurrentDictionary<string, TSerialized> Convert<TValue, TSerialized>(this ConcurrentDictionary<string, TValue> dict, Func<TValue, TSerialized> converter) {
        var newDict = new ConcurrentDictionary<string, TSerialized>();
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
