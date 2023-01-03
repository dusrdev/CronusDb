using BenchmarkDotNet.Attributes;

namespace CronusDb.Benchmarks;

[MemoryDiagnoser]
public class DictBenchmarks {
    private Dictionary<string, User>? UserDb { get; set; }
    private int num = 1;

    [GlobalSetup]
    public void Setup() {
        UserDb = new();
    }

    [Benchmark]
    public void Upsert() {
        var user = new User {
            Age = num,
            Name = "David",
            DateOfBirth = DateTime.Now
        };
        UserDb![num.ToString()] = user;
        num++;
    }
}
