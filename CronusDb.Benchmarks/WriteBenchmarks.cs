using BenchmarkDotNet.Attributes;

namespace CronusDb.Benchmarks;

[MemoryDiagnoser]
public class WriteBenchmarks {
    private CronusDb<User>? UserDb { get; set; }
    private int num = 1;

    [GlobalSetup]
    public async Task Setup() {
        UserDb = await CronusDb<User>.Create();
    }

    [Benchmark]
    public void Upsert() {
        var user = new User {
            Age = num,
            Name = "David",
            DateOfBirth = DateTime.Now
        };
        UserDb!.Upsert(num.ToString(), user);
        num++;
    }
}
