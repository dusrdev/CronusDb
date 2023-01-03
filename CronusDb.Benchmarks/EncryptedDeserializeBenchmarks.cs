using BenchmarkDotNet.Attributes;

using System.Text.Json;

namespace CronusDb.Benchmarks;

[MemoryDiagnoser]
public class EncryptedDeserializeBenchmarks {
    private CronusDb<User>? UserDb { get; set; }
    private CronusDb<User>? EncryptedDb { get; set; }

    private readonly CronusDbConfiguration<User> _userConfig = new CronusDbConfiguration<User>() {
        Path = @".\User.db",
        Serializer = static x => JsonSerializer.Serialize(x, JsonContext.Default.User),
        Deserializer = static x => JsonSerializer.Deserialize(x, JsonContext.Default.User)
    };

    private readonly CronusDbConfiguration<User> _encryptedConfig = new CronusDbConfiguration<User>() {
        Path = @".\Encrypted.db",
        EncryptionKey = "1q2w3e4r5t",
        Serializer = static x => JsonSerializer.Serialize(x, JsonContext.Default.User),
        Deserializer = static x => JsonSerializer.Deserialize(x, JsonContext.Default.User)
    };

    [GlobalSetup]
    public async Task Setup() {
        UserDb = await CronusDb<User>.Create(_userConfig);

        EncryptedDb = await CronusDb<User>.Create(_encryptedConfig);

        foreach (var num in Enumerable.Range(1, 1001)) {
            var user = new User {
                Age = num,
                Name = "David",
                DateOfBirth = DateTime.Now
            };
            UserDb.Upsert(num.ToString(), user);
            EncryptedDb.Upsert(num.ToString(), user);
        }

        await UserDb!.Serialize();
        await EncryptedDb!.Serialize();
    }

    [Benchmark(Baseline = true)]
    public async Task Deserialize() => _ = await CronusDb<User>.Create(_userConfig);

    [Benchmark]
    public async Task DeserializeEncrypted() => _ = await CronusDb<User>.Create(_encryptedConfig);
}
