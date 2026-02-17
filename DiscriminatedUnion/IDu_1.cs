using System.Text.Json;

namespace NickStrupat;

internal interface IDu<TDu> : IDu where TDu : struct, IDu<TDu>
{
	static virtual TDu Deserialize(ref Utf8JsonReader reader, JsonSerializerOptions options) => throw new NotImplementedException();
}