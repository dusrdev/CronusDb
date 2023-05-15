using System.Text;
using System.Text.Json;

namespace CronusDbTests;

internal static class Extensions {
    public static byte[] ToByteArray(this string str) => Encoding.UTF8.GetBytes(str);

    public static string ToUTF8String(this byte[] bytes) => Encoding.UTF8.GetString(bytes);
}
