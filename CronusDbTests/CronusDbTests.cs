using CronusDb;

using FluentAssertions;

using Xunit;

namespace CronusDbTests;

public class CronusDbTests {
    [Fact]
    public async Task Database_CreateAddSerializeDeserialize() {
        var config = new DatabaseConfiguration<int>() {
            Path = @".\temp.db",
            ToTSerialized = static x => x.ToString().ToByteArray(),
            ToTValue = static x => int.Parse(x.ToUTF8String())
        };

        var db = await Cronus.CreateDatabaseAsync(config);

        db.Upsert("David", 25);
        db.Upsert("Ben", 28);
        db.Upsert("Nick", 37);
        db.Upsert("Alex", 63);

        await db.SerializeAsync();

        var rdb = await Cronus.CreateDatabaseAsync(config);

        rdb.Get("David").Should().Be(25);
        rdb.Get("Ben").Should().Be(28);
        rdb.Get("Nick").Should().Be(37);
        rdb.Get("Alex").Should().Be(63);
    }

    [Fact]
    public async Task DatabaseIgnoreKeyCase_CreateAddSerializeDeserialize() {
        var config = new DatabaseConfiguration<int>() {
            Path = @".\temp.db",
            Options = DatabaseOptions.IgnoreKeyCases,
            ToTSerialized = static x => x.ToString().ToByteArray(),
            ToTValue = static x => int.Parse(x.ToUTF8String())
        };

        var db = await Cronus.CreateDatabaseAsync(config);

        db.Upsert("David", 25);
        db.Upsert("Ben", 28);
        db.Upsert("Nick", 37);
        db.Upsert("Alex", 63);

        await db.SerializeAsync();

        var rdb = await Cronus.CreateDatabaseAsync(config);

        rdb.Get("daViD").Should().Be(25);
        rdb.Get("bEN").Should().Be(28);
        rdb.Get("NiCK").Should().Be(37);
        rdb.Get("aLEX").Should().Be(63);
    }

    [Fact]
    public void DatabaseEncrypted_CreateAddSerializeDeserialize() {
        var config = new DatabaseConfiguration<int>() {
            Path = @".\encrypted.db",
            EncryptionKey = "1q2w3e4r5t",
            ToTSerialized = static x => x.ToString().ToByteArray(),
            ToTValue = static x => int.Parse(x.ToUTF8String())
        };

        var db = Cronus.CreateDatabase(config);

        db.Upsert("David", 25);
        db.Upsert("Ben", 28);
        db.Upsert("Nick", 37);
        db.Upsert("Alex", 63);

        db.Serialize();

        var rdb = Cronus.CreateDatabase(config);

        rdb.Get("David").Should().Be(25);
        rdb.Get("Ben").Should().Be(28);
        rdb.Get("Nick").Should().Be(37);
        rdb.Get("Alex").Should().Be(63);
    }

    [Fact]
    public void RemoveAnyTest() {
        var db = Cronus.CreateDatabase(new DatabaseConfiguration<int>() {
            Path = @".\temp.db",
            ToTSerialized = static x => x.ToString().ToByteArray(),
            ToTValue = static x => int.Parse(x.ToUTF8String())
        });

        db.Upsert("David", 25);
        db.Upsert("Ben", 28);
        db.Upsert("Nick", 37);
        db.Upsert("Alex", 63);

        db.RemoveAny(static x => x > 30);

        db.ContainsKey("David").Should().BeTrue();
        db.ContainsKey("Ben").Should().BeTrue();
        db.ContainsKey("Nick").Should().BeFalse();
        db.ContainsKey("Alex").Should().BeFalse();
    }

    [Fact]
    public void UpsertEventTest() {
        var db = Cronus.CreateDatabase(new DatabaseConfiguration<int>() {
            Path = @".\temp.db",
            Options = DatabaseOptions.TriggerUpdateEvents,
            ToTSerialized = static x => x.ToString().ToByteArray(),
            ToTValue = static x => int.Parse(x.ToUTF8String())
        });

        bool triggered = false;

        db.DataChanged += (_, _) => triggered = true;

        db.Upsert("David", 25);

        triggered.Should().BeTrue();
    }

    [Fact]
    public void RemoveEventTest() {
        var db = Cronus.CreateDatabase(new DatabaseConfiguration<int>() {
            Path = @".\temp.db",
            Options = DatabaseOptions.TriggerUpdateEvents,
            ToTSerialized = static x => x.ToString().ToByteArray(),
            ToTValue = static x => int.Parse(x.ToUTF8String())
        });

        bool triggered = false;

        db.Upsert("David", 25);

        db.DataChanged += (_, e) => {
            if (e.ChangeType is DataChangeType.Remove) {
                triggered = true;
            }
        };

        db.Remove("David");

        triggered.Should().BeTrue();
    }
}