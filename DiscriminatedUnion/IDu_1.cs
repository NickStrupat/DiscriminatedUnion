using System.Text.Json;

namespace NickStrupat;

internal interface IDu<TDu> : IDu where TDu : IDu<TDu>
{
	public Boolean TryPick<T>(out T matched) where T : notnull;
	void Visit<T>(Action<T> action) where T : notnull;

	static abstract TDu Deserialize(ref Utf8JsonReader reader, JsonSerializerOptions? options);
	void Serialize(Utf8JsonWriter writer, JsonSerializerOptions options);
}