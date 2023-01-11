using System.Security.Cryptography;
using System.Text;

namespace CronusDb;

internal sealed class CronusAesProvider : IDisposable {
    private static readonly byte[] _vector = { 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 23, 19, 17 };
    private readonly Aes _aes;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="strKey">Encryption key as string</param>
    public CronusAesProvider(string strKey) {
        _aes = Aes.Create();
        _aes.KeySize = 256;
        _aes.Padding = PaddingMode.PKCS7;
        _aes.Key = CreateKey(strKey);
        _aes.IV = _vector;
        _aes.Mode = CipherMode.CBC;
    }

    // Creates a usable fixed length key from the string password
    private static byte[] CreateKey(string strKey) => SHA256.HashData(Encoding.UTF8.GetBytes(strKey));

    /// <summary>
    /// Encrypts the text using the key
    /// </summary>
    /// <param name="unencrypted">original text</param>
    /// <returns>Unicode string</returns>
    public string Encrypt(string unencrypted) {
        var buffer = Encoding.UTF8.GetBytes(unencrypted);
        var result = _aes.EncryptCbc(buffer, _aes.IV);
        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypts encrypted text
    /// </summary>
    /// <param name="encrypted">Encrypted text</param>
    /// <returns>Transformed Unicode string</returns>
    public string Decrypt(string encrypted) {
        var buffer = Convert.FromBase64String(encrypted);
        var result = _aes.DecryptCbc(buffer, _aes.IV);
        return Encoding.UTF8.GetString(result);
    }

    public void Dispose() {
        _aes?.Dispose();
    }
}
