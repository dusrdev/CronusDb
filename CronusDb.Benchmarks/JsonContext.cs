using System.Text.Json.Serialization;

namespace CronusDb.Benchmarks;

[JsonSourceGenerationOptions(IncludeFields = true)]
[JsonSerializable(typeof(User))]
internal partial class JsonContext : JsonSerializerContext {
}
