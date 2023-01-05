using BenchmarkDotNet.Attributes;

using System.Text.Json;

namespace CronusDb.Benchmarks;

[MemoryDiagnoser]
public class EncryptedDeserializeBenchmarks {
    private SerializableDatabase<User>? UserDb { get; set; }
    private SerializableDatabase<User>? EncryptedDb { get; set; }

    private readonly SerializableDatabaseConfiguration<User> _userConfig = new () {
        Path = @".\User.db",
        ToStringConverter = static x => JsonSerializer.Serialize(x, JsonContext.Default.User),
        FromStringConverter = static x => JsonSerializer.Deserialize(x, JsonContext.Default.User)
    };

    private readonly SerializableDatabaseConfiguration<User> _encryptedConfig = new () {
        Path = @".\Encrypted.db",
        EncryptionKey = "1q2w3e4r5t",
        ToStringConverter = static x => JsonSerializer.Serialize(x, JsonContext.Default.User),
        FromStringConverter = static x => JsonSerializer.Deserialize(x, JsonContext.Default.User)
    };

    [GlobalSetup]
    public async Task Setup() {
        UserDb = await CronusDatabase.CreateSerializableDatabaseAsync(_userConfig);

        EncryptedDb = await CronusDatabase.CreateSerializableDatabaseAsync(_encryptedConfig);

        foreach (var num in Enumerable.Range(1, 1001)) {
            var user = new User {
                Age = num,
                Name = "David",
                DateOfBirth = DateTime.Now
            };
            UserDb.Upsert(num.ToString(), user);
            EncryptedDb.Upsert(num.ToString(), user);
        }

        await UserDb!.SerializeAsync();
        await EncryptedDb!.SerializeAsync();
    }

    [Benchmark(Baseline = true)]
    public async Task Deserialize() => _ = await CronusDatabase.CreateSerializableDatabaseAsync(_userConfig);

    [Benchmark]
    public async Task DeserializeEncrypted() => _ = await CronusDatabase.CreateSerializableDatabaseAsync(_encryptedConfig);
}
