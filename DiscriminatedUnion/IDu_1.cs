using System.Text.Json;

namespace NickStrupat;

internal interface IDu<TDu> : IDu where TDu : IDu<TDu>
{
	static abstract TDu TryDeserialize(ref Utf8JsonReader reader, JsonSerializerOptions? options);
	void Serialize(Utf8JsonWriter writer, JsonSerializerOptions options);
}