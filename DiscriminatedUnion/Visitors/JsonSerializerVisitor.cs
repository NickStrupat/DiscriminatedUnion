using System.Text.Json;

namespace NickStrupat;

internal readonly struct JsonSerializerVisitor(Utf8JsonWriter writer, JsonSerializerOptions options) : IVisitor
{
	void IVisitor.Visit<T>(T value) => JsonSerializer.Serialize(writer, value, options);
}