using System.Text.Json;
using System.Text.Json.Serialization;

namespace NickStrupat;

[JsonConverter(typeof(JsonConverter))]
public struct None : IEquatable<None>
{
	private sealed class JsonConverter : JsonConverter<None>
	{
		public override None Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.TokenType == JsonTokenType.Null
				? default
				: throw new JsonException();

		public override void Write(Utf8JsonWriter writer, None value, JsonSerializerOptions options) =>
			writer.WriteNullValue();
	}

	public Boolean Equals(None other) => true;
	public override Boolean Equals(Object? obj) => obj is None;
	public override Int32 GetHashCode() => 0;

	public static Boolean operator ==(None left, None right) => left.Equals(right);
	public static Boolean operator !=(None left, None right) => !left.Equals(right);
}