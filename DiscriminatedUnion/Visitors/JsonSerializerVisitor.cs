using System.Text.Json;

namespace NickStrupat;

internal readonly struct JsonSerializerVisitor(Utf8JsonWriter writer, JsonSerializerOptions options) : IVisitor<Null>
{
	Null IVisitor<Null>.Visit<T>(T value)
	{
		JsonSerializer.Serialize(writer, value, options);
		return default;
	}
}