using System.Text.Json.Serialization;

namespace CronusDb;

[JsonSourceGenerationOptions(IncludeFields = true)]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class JsonContexts : JsonSerializerContext {
}
