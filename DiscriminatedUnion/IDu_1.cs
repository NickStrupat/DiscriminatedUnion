using System.Text.Json;

namespace NickStrupat;

public interface IDu<TDu> : IDu where TDu : IDu<TDu>
{
	static virtual TDu Deserialize(ref Utf8JsonReader reader, JsonSerializerOptions options) => throw new NotImplementedException();
	void Serialize(Utf8JsonWriter writer, JsonSerializerOptions options);
}