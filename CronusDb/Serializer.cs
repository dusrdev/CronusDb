using MemoryPack;

using System.Collections.Concurrent;

namespace CronusDb;

internal static class Serializer {
    internal static void Serialize<TSerialized>(ConcurrentDictionary<string, TSerialized> data, string path, string? encryptionKey) {
        var bin = MemoryPackSerializer.Serialize(data);
        File.WriteAllBytes(path, string.IsNullOrWhiteSpace(encryptionKey) ? bin : bin.Encrypt(encryptionKey));
    }

    internal static async Task SerializeAsync<TSerialized>(ConcurrentDictionary<string, TSerialized> data, string path, string? encryptionKey) {
        var bin = MemoryPackSerializer.Serialize(data);
        await File.WriteAllBytesAsync(path, string.IsNullOrWhiteSpace(encryptionKey) ? bin : bin.Encrypt(encryptionKey));
    }

    internal static ConcurrentDictionary<string, TSerialized> Convert<TValue, TSerialized>(this ConcurrentDictionary<string, TValue> dict, Func<TValue, TSerialized> converter) {
        var newDict = new ConcurrentDictionary<string, TSerialized>();
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
