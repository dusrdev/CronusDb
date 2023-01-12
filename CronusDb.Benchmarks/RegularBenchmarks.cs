using BenchmarkDotNet.Attributes;

using System.Text.Json;

namespace CronusDb.Benchmarks;

[MemoryDiagnoser]
public class RegularBenchmarks {
    private SerializableDatabase<User>? UserDb { get; set; }

    [GlobalSetup]
    public async Task Setup() {
        UserDb = await Cronus.CreateSerializableDatabaseAsync(new SerializableDatabaseConfiguration<User>() {
            Path = @".\User.db",
            ToStringConverter = static x => JsonSerializer.Serialize(x, JsonContext.Default.User),
            FromStringConverter = static x => JsonSerializer.Deserialize(x, JsonContext.Default.User)
        });

        foreach (var num in Enumerable.Range(1, 1001)) {
            UserDb.Upsert(num.ToString(), new User {
                Age = num,
                Name = "David",
                DateOfBirth = DateTime.Now
            });
        }
    }

    [Benchmark]
    public async Task Serialize() => await UserDb!.SerializeAsync();
}
