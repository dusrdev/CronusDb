using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CronusDb.Tests;

[TestClass]
public class CronusDbTests {
    [TestMethod]
    public async Task GeneralTest_CreateAddSerializeDeserialize() {
        var config = new GenericDatabaseConfiguration<int, string>() {
            Path = @".\temp.db",
            ToTSerialized = static x => x.ToString(),
            ToTValue = static x => int.Parse(x)
        };

        var db = await Cronus.CreateGenericDatabaseAsync(config);

        db.Upsert("David", 25);
        db.Upsert("Ben", 28);
        db.Upsert("Nick", 37);
        db.Upsert("Alex", 63);

        await db.SerializeAsync();

        var rdb = await Cronus.CreateGenericDatabaseAsync(config);

        Assert.AreEqual(25, rdb.Get("David"));
        Assert.AreEqual(28, rdb.Get("Ben"));
        Assert.AreEqual(37, rdb.Get("Nick"));
        Assert.AreEqual(63, rdb.Get("Alex"));
    }

    [TestMethod]
    public void GeneralTestEncrypted_CreateAddSerializeDeserialize() {
        var config = new GenericDatabaseConfiguration<int, string>() {
            Path = @".\encrypted.db",
            EncryptionKey = "1q2w3e4r5t",
            ToTSerialized = static x => x.ToString(),
            ToTValue = static x => int.Parse(x)
        };

        var db = Cronus.CreateGenericDatabase(config);

        db.Upsert("David", 25);
        db.Upsert("Ben", 28);
        db.Upsert("Nick", 37);
        db.Upsert("Alex", 63);

        db.Serialize();

        var rdb = Cronus.CreateGenericDatabase(config);

        Assert.AreEqual(25, rdb.Get("David"));
        Assert.AreEqual(28, rdb.Get("Ben"));
        Assert.AreEqual(37, rdb.Get("Nick"));
        Assert.AreEqual(63, rdb.Get("Alex"));
    }

    [TestMethod]
    public void RemoveAnyTest() {
        var db = Cronus.CreateGenericDatabase(new GenericDatabaseConfiguration<int, string>() {
            Path = @".\temp.db",
            ToTSerialized = static x => x.ToString(),
            ToTValue = static x => int.Parse(x)
        });

        db.Upsert("David", 25);
        db.Upsert("Ben", 28);
        db.Upsert("Nick", 37);
        db.Upsert("Alex", 63);

        db.RemoveAny(static x => x > 30);

        Assert.IsTrue(db.ContainsKey("David"));
        Assert.IsTrue(db.ContainsKey("Ben"));
        Assert.IsFalse(db.ContainsKey("Nick"));
        Assert.IsFalse(db.ContainsKey("Alex"));
    }

    [TestMethod]
    public void UpsertEventTest() {
        var db = Cronus.CreateGenericDatabase(new GenericDatabaseConfiguration<int, string>() {
            Path = @".\temp.db",
            Options = DatabaseOptions.TriggerUpdateEvents,
            ToTSerialized = static x => x.ToString(),
            ToTValue = static x => int.Parse(x)
        });

        bool triggered = false;

        db.DataChanged += (_, _) => triggered = true;

        db.Upsert("David", 25);

        Assert.IsTrue(triggered);
    }

    [TestMethod]
    public void RemoveEventTest() {
        var db = Cronus.CreateGenericDatabase(new GenericDatabaseConfiguration<int, string>() {
            Path = @".\temp.db",
            Options = DatabaseOptions.TriggerUpdateEvents,
            ToTSerialized = static x => x.ToString(),
            ToTValue = static x => int.Parse(x)
        });

        bool triggered = false;

        db.Upsert("David", 25);

        db.DataChanged += (_, e) => {
            if (e.ChangeType is DataChangeType.Remove) {
                triggered = true;
            }
        };

        db.Remove("David");

        Assert.IsTrue(triggered);
    }
}