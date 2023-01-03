using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CronusDb.Tests {
    [TestClass]
    public class CronusDbTests {
        [TestMethod]
        public async Task GeneralTest_CreateAddSerializeDeserialize() {
            var config = new CronusDbConfiguration<int>() {
                Path = @".\temp.db",
                Serializer = static x => x.ToString(),
                Deserializer = static x => int.Parse(x)
            };

            var db = await CronusDb<int>.Create(config);

            db.Upsert("David", 25);
            db.Upsert("Ben", 28);
            db.Upsert("Nick", 37);
            db.Upsert("Alex", 63);

            await db.Serialize();

            var rdb = await CronusDb<int>.Create(config);

            Assert.AreEqual(25, rdb.Get("David"));
            Assert.AreEqual(28, rdb.Get("Ben"));
            Assert.AreEqual(37, rdb.Get("Nick"));
            Assert.AreEqual(63, rdb.Get("Alex"));
        }

        [TestMethod]
        public async Task GeneralTestEncrypted_CreateAddSerializeDeserialize() {
            var config = new CronusDbConfiguration<int>() {
                Path = @".\encrypted.db",
                EncryptionKey = "1q2w3e4r5t",
                Serializer = static x => x.ToString(),
                Deserializer = static x => int.Parse(x)
            };

            var db = await CronusDb<int>.Create(config);

            db.Upsert("David", 25);
            db.Upsert("Ben", 28);
            db.Upsert("Nick", 37);
            db.Upsert("Alex", 63);

            await db.Serialize();

            var rdb = await CronusDb<int>.Create(config);

            Assert.AreEqual(25, rdb.Get("David"));
            Assert.AreEqual(28, rdb.Get("Ben"));
            Assert.AreEqual(37, rdb.Get("Nick"));
            Assert.AreEqual(63, rdb.Get("Alex"));
        }

        [TestMethod]
        public async Task UpsertTest() {
            var db = await CronusDb<int>.Create();

            db.Upsert("David", 25);

            Assert.IsTrue(db.ContainsKey("David"));
        }

        [TestMethod]
        public async Task RemoveTest() {
            var db = await CronusDb<int>.Create();

            db.Upsert("David", 25);

            db.Remove("David");

            Assert.IsFalse(db.ContainsKey("David"));
        }

        [TestMethod]
        public async Task RemoveAnyTest() {
            var db = await CronusDb<int>.Create();

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
        public async Task UpsertEventTest() {
            var db = await CronusDb<int>.Create();

            bool triggered = false;

            db.ItemUpserted += (_, _) => triggered = true;

            db.Upsert("David", 25);

            Assert.IsTrue(triggered);
        }

        [TestMethod]
        public async Task RemoveEventTest() {
            var db = await CronusDb<int>.Create();

            bool triggered = false;

            db.Upsert("David", 25);

            db.ItemRemoved += (_, _) => triggered = true;

            db.Remove("David");

            Assert.IsTrue(triggered);
        }
    }
}