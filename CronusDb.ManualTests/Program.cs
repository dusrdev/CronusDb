using CronusDb;

using System.Diagnostics;

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
