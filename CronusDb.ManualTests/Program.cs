using CronusDb;

using System.Diagnostics;

string[] originals = {
            "David",
            """{["ben", "alex"]}""",
            "dasdasd\"sadad"
        };

var aes = new CronusAesProvider("1q2w3e4r5t");

var processed = originals.Select(aes.Encrypt).ToArray();
var decrypted = processed.Select(aes.Decrypt).ToArray();

for (int i = 0; i < originals.Length; i++) {
    Console.WriteLine($"Original: {originals[i]}");
    Console.WriteLine($"Processed: {processed[i]}");
    Console.WriteLine($"Decrypted: {decrypted[i]}");
    Console.WriteLine("");
    Debug.Assert(originals[i] == decrypted[i]);
}

//var config = new CronusDbConfiguration<int>() {
//    Path = @".\encrypted.db",
//    EncryptionKey = "1q2w3e4r5t",
//    ToStringConverter = static x => x.ToString(),
//    FromStringConverter = static x => int.Parse(x)
//};

//var db = await CronusDatabase<int>.Create(config);

//db.Upsert("David", 25);

//await db.SerializeAsync();

//var rdb = await CronusDatabase<int>.Create(config);

//Console.WriteLine($"David => {rdb.Get("David")}");

Console.ReadKey();
