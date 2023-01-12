using BenchmarkDotNet.Attributes;

namespace CronusDb.Benchmarks;

[MemoryDiagnoser]
public class WriteBenchmarks {
    private InMemoryDatabase<User>? UserDb { get; set; }
    private int num = 1;

    [GlobalSetup]
    public void Setup() {
        UserDb = Cronus.CreateInMemoryDatabase<User>();
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
