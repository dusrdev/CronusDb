using BenchmarkDotNet.Attributes;

using System.Text.Json;

namespace CronusDb.Benchmarks;

[MemoryDiagnoser]
public class RegularBenchmarks {
    private CronusDb<int>? PrimitiveDb { get; set; }
    private CronusDb<User>? UserDb { get; set; }

    [GlobalSetup]
    public async Task Setup() {
        PrimitiveDb = await CronusDb<int>.Create(new CronusDbConfiguration<int>() {
            Path = @".\Primitive.db",
            Serializer = static x => x.ToString(),
            Deserializer = static x => int.Parse(x)
        });

        UserDb = await CronusDb<User>.Create(new CronusDbConfiguration<User>() {
            Path = @".\Primitive.db",
            Serializer = static x => JsonSerializer.Serialize(x, JsonContext.Default.User),
            Deserializer = static x => JsonSerializer.Deserialize(x, JsonContext.Default.User)
        });

        foreach (var num in Enumerable.Range(1, 1001)) {
            PrimitiveDb.Upsert(num.ToString(), num);
            UserDb.Upsert(num.ToString(), new User {
                Age = num,
                Name = "David",
                DateOfBirth = DateTime.Now
            });
        }
    }

    [Benchmark]
    public async Task Serialize() => await UserDb!.Serialize();
}
