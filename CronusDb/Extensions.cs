namespace CronusDb;

internal static class Extensions {
    internal static string Encrypt(this string value, string key) {
        using var aes = new CronusAesProvider(key);
        return aes.Encrypt(value);
    }

    internal static string Decrypt(this string value, string key) {
        using var aes = new CronusAesProvider(key);
        return aes.Decrypt(value);
    }
}
