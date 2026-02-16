using System.Text.Json;

namespace NickStrupat;

internal readonly struct JsonSerializerVisitor(Utf8JsonWriter writer, JsonSerializerOptions options) : IVisitor<None>
{
	None IVisitor<None>.Visit<T>(T value)
	{
		JsonSerializer.Serialize(writer, value, options);
		return default;
	}
}