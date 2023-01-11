using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CronusDbTests;

[TestClass]
public class EncryptionTests {
    [TestMethod]
    public void EncryptionTest() {
        string[] originals = {
            "David",
            """{["ben", "alex"]}""",
            "dasdasd\"sadad"
        };

        var aes = new CronusDb.CronusAesProvider("1q2w3e4r5t");

        var processed = originals.Select(aes.Encrypt).ToArray();
        var decrypted = processed.Select(aes.Decrypt).ToArray();

        CollectionAssert.AreEqual(originals, decrypted);
    }
}
