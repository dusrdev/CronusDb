using BenchmarkDotNet.Attributes;

using System.Text.Json;

namespace CronusDb.Benchmarks;

[MemoryDiagnoser]
public class ConvertedBenchmarks {
    private InMemoryDatabase<User> _regularDb;

    private SerializableDatabase<User> _convertedDb;

    [GlobalSetup]
    public async Task Setup() {
        _regularDb = CronusDb.CreateInMemoryDatabase<User>();
        _convertedDb = await CronusDb.CreateSerializableDatabaseAsync(new SerializableDatabaseConfiguration<User> {
            Path = "",
            ToStringConverter = static x => JsonSerializer.Serialize(x, JsonContext.Default.User),
            FromStringConverter = static x => JsonSerializer.Deserialize(x, JsonContext.Default.User)
        });

        foreach (var i in Enumerable.Range(501, 601)) {
            var user = new User {
                Name = i.ToString(),
                Age = i,
                DateOfBirth = DateTime.Now
            };

            _regularDb.Upsert(i.ToString(), user);
            _convertedDb.Upsert(i.ToString(), user);
        }
    }

    [Benchmark]
    public void WriteRegular() {
        foreach (var i in Enumerable.Range(1, 101)) {
            _regularDb.Upsert(i.ToString(), new User {
                Name = i.ToString(),
                Age = i,
                DateOfBirth = DateTime.Now
            });
        }
    }

    [Benchmark]
    public void WriteConverted() {
        foreach (var i in Enumerable.Range(1, 101)) {
            _convertedDb.Upsert(i.ToString(), new User {
                Name = i.ToString(),
                Age = i,
                DateOfBirth = DateTime.Now
            });
        }
    }

    [Benchmark]
    public void ReadRegular() {
        foreach (var i in Enumerable.Range(501, 601)) {
             _ = _regularDb.Get(i.ToString());
        }
    }

    [Benchmark]
    public void ReadConverted() {
        foreach (var i in Enumerable.Range(501, 601)) {
            _ = _convertedDb.Get(i.ToString());
        }
    }
}
