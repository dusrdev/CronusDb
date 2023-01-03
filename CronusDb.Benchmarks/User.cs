namespace CronusDb.Benchmarks;

public record struct User {
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime DateOfBirth { get; set; }
}
