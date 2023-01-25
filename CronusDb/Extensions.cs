using System.Text;

namespace CronusDb;

internal static class Extensions {
    internal static byte[] Encrypt(this byte[] value, string key) {
        using var aes = new CronusAesProvider(key);
        return aes.Encrypt(value);
    }

    internal static byte[] Decrypt(this byte[] value, string key) {
        using var aes = new CronusAesProvider(key);
        return aes.Decrypt(value);
    }

    public static byte[] ToByteArray(this string str) => Encoding.UTF8.GetBytes(str);

    public static string ToUTF8String(this byte[] bytes) => Encoding.UTF8.GetString(bytes);
}
