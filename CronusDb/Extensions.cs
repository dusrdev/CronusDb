namespace CronusDb;

internal static class Extensions {
    internal static byte[] Encrypt(this byte[] value, string key) {
        using var aes = new CronusAesProvider(key);
        return aes.Encrypt(value);
    }

    internal static string Encrypt(this string value, string key) {
        using var aes = new CronusAesProvider(key);
        return aes.Encrypt(value);
    }

    internal static byte[] Decrypt(this ReadOnlySpan<byte> value, string key) {
        using var aes = new CronusAesProvider(key);
        return aes.Decrypt(value);
    }

    internal static string Decrypt(this string value, string key) {
        using var aes = new CronusAesProvider(key);
        return aes.Decrypt(value);
    }
}
