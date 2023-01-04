using BenchmarkDotNet.Attributes;

namespace CronusDb.Benchmarks;

[MemoryDiagnoser]
public class ReadBenchmarks {
    private InMemoryDatabase<User>? UserDb { get; set; }

    [GlobalSetup]
    public void Setup() {
        UserDb = CronusDb.CreateInMemoryDatabase<User>();

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
