using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace CronusDb;

internal sealed class CronusAesProvider : IDisposable {
    private static readonly byte[] Vector = { 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 23, 19, 17 };
    private readonly Aes _aes;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="strKey">Encryption key as string</param>
    public CronusAesProvider(string strKey) {
        _aes = Aes.Create();
        _aes.KeySize = 256;
        _aes.BlockSize = 128;
        _aes.FeedbackSize = 8;
        _aes.Padding = PaddingMode.PKCS7;
        _aes.Key = CreateKey(strKey);
        _aes.IV = Vector;
        _aes.Mode = CipherMode.CBC;
    }

    // Creates a usable fixed length key from the string password
    private static byte[] CreateKey(string strKey) => SHA256.HashData(Encoding.UTF8.GetBytes(strKey));

    /// <summary>
    /// Encrypts the bytes using the key
    /// </summary>
    /// <param name="unencrypted">original text</param>
    /// <returns>Unicode string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] Encrypt(byte[] unencrypted) => _aes.EncryptCbc(unencrypted, _aes.IV);

    /// <summary>
    /// Encrypts the text using the key
    /// </summary>
    /// <param name="unencrypted">original text</param>
    /// <returns>Unicode string</returns>
    public string Encrypt(string unencrypted) {
        var buffer = Encoding.UTF8.GetBytes(unencrypted);
        var result = Encrypt(buffer);
        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypts encrypted bytes
    /// </summary>
    /// <param name="encrypted">Encrypted text</param>
    /// <returns>Transformed Unicode string, or empty if it failed</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] Decrypt(ReadOnlySpan<byte> encrypted) {
        try {
            return _aes.DecryptCbc(encrypted, _aes.IV);
        } catch (CryptographicException) {
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// Decrypts encrypted text
    /// </summary>
    /// <param name="encrypted">Encrypted text</param>
    /// <returns>Transformed Unicode string</returns>
    public string Decrypt(string encrypted) {
        var buffer = Convert.FromBase64String(encrypted);
        var result = Decrypt(buffer);
        return result.Length is 0
            ? string.Empty
            : Encoding.UTF8.GetString(result);
    }

    public void Dispose() {
        _aes?.Dispose();
    }
}
