using System.Security.Cryptography;
using System.Text;

namespace CronusDb;

internal sealed class CronusAesProvider : IDisposable {
    private readonly byte[] _vector = { 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 23, 19, 17 };
    private readonly ICryptoTransform? _encryptor;
    private readonly ICryptoTransform? _decryptor;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="strKey">Encryption key as string</param>
    public CronusAesProvider(string strKey) {
        var _myAes = Aes.Create();
        _myAes.KeySize = 256;
        _myAes.Padding = PaddingMode.PKCS7;
        _myAes.Key = CreateKey(strKey);
        _myAes.IV = _vector;
        _encryptor = _myAes.CreateEncryptor(_myAes.Key, _myAes.IV);
        _decryptor = _myAes.CreateDecryptor(_myAes.Key, _myAes.IV);
    }

    // Creates a usable fixed length key from the string password
    private static byte[] CreateKey(string strKey) {
        return SHA256.HashData(Encoding.Unicode.GetBytes(strKey));
    }

    public ICryptoTransform GetEncrypter() => _encryptor!;

    public ICryptoTransform? GetDecrypter() => _decryptor!;

    public void Dispose() {
        _encryptor?.Dispose();
        _decryptor?.Dispose();
    }
}
