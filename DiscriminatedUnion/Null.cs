using System.Text.Json;
using System.Text.Json.Serialization;

namespace NickStrupat;

[JsonConverter(typeof(NullJsonConverter))]
public struct Null
{
	private class NullJsonConverter : JsonConverter<Null>
	{
		public override Null Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.TokenType == JsonTokenType.Null
				? default
				: throw new JsonException();

		public override void Write(Utf8JsonWriter writer, Null value, JsonSerializerOptions options) =>
			writer.WriteNullValue();
	}
}