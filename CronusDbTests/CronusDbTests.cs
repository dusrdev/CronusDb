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

            db.SetValue("David", 25);
            db.SetValue("Irit", 61);
            db.SetValue("Helena", 37);
            db.SetValue("Alex", 63);

            await db.Serialize();

            var rdb = await CronusDb<int>.Create(config);

            Assert.AreEqual(25, rdb.GetValue("David"));
            Assert.AreEqual(61, rdb.GetValue("Irit"));
            Assert.AreEqual(37, rdb.GetValue("Helena"));
            Assert.AreEqual(63, rdb.GetValue("Alex"));
        }
    }
}