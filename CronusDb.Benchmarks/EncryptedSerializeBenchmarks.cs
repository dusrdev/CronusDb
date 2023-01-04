using BenchmarkDotNet.Attributes;

using System.Text.Json;

namespace CronusDb.Benchmarks;

[MemoryDiagnoser]
public class EncryptedSerializeBenchmarks {
    private SerializableDatabase<User>? UserDb { get; set; }
    private SerializableDatabase<User>? EncryptedDb { get; set; }

    [GlobalSetup]
    public async Task Setup() {
        UserDb = await CronusDb.CreateSerializableDatabase(new SerializableDatabaseConfiguration<User>() {
            Path = @".\User.db",
            ToStringConverter = static x => JsonSerializer.Serialize(x, JsonContext.Default.User),
            FromStringConverter = static x => JsonSerializer.Deserialize(x, JsonContext.Default.User)
        });

        EncryptedDb = await CronusDb.CreateSerializableDatabase(new SerializableDatabaseConfiguration<User>() {
            Path = @".\Encrypted.db",
            EncryptionKey = "1q2w3e4r5t",
            ToStringConverter = static x => JsonSerializer.Serialize(x, JsonContext.Default.User),
            FromStringConverter = static x => JsonSerializer.Deserialize(x, JsonContext.Default.User)
        });

        foreach (var num in Enumerable.Range(1, 1001)) {
            var user = new User {
                Age = num,
                Name = "David",
                DateOfBirth = DateTime.Now
            };
            UserDb.Upsert(num.ToString(), user);
            EncryptedDb.Upsert(num.ToString(), user);
        }
    }

    [Benchmark(Baseline = true)]
    public async Task Serialize() => await UserDb!.Serialize();

    [Benchmark]
    public async Task SerializeEncrypted() => await EncryptedDb!.Serialize();
}
