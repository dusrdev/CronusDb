using CronusDb;

var config = new CronusDbConfiguration<int>() {
    Path = @".\encrypted.db",
    EncryptionKey = "1q2w3e4r5t",
    Serializer = static x => x.ToString(),
    Deserializer = static x => int.Parse(x)
};

//var db = await CronusDb<int>.Create(config);

//db.Upsert("David", 25);

//await db.Serialize();

//var rdb = await CronusDb<int>.Create(config);

//Console.WriteLine($"David => {rdb.Get("David")}");
