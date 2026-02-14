using System.Text.Json;
using System.Text.Json.Serialization;

namespace NickStrupat;

[JsonConverter(typeof(JsonConverter))]
public struct Null : IEquatable<Null>
{
	private sealed class JsonConverter : JsonConverter<Null>
	{
		public override Null Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.TokenType == JsonTokenType.Null
				? default
				: throw new JsonException();

		public override void Write(Utf8JsonWriter writer, Null value, JsonSerializerOptions options) =>
			writer.WriteNullValue();
	}

	public Boolean Equals(Null other) => true;
	public override Boolean Equals(Object? obj) => obj is Null;
	public override Int32 GetHashCode() => 0;

	public static Boolean operator ==(Null left, Null right) => left.Equals(right);
	public static Boolean operator !=(Null left, Null right) => !left.Equals(right);
}