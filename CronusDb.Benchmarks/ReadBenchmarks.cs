using BenchmarkDotNet.Attributes;

namespace CronusDb.Benchmarks;

[MemoryDiagnoser]
public class ReadBenchmarks {
    private CronusDb<User>? UserDb { get; set; }

    [GlobalSetup]
    public async Task Setup() {
        UserDb = await CronusDb<User>.Create();

        var user = new User {
            Age = 500,
            Name = "David",
            DateOfBirth = DateTime.Now
        };
        UserDb!.Upsert("number", user);
    }

    [Benchmark]
    public void Upsert() => _ = UserDb!.Get("number");
}
