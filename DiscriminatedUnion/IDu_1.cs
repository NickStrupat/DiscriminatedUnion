using System.Text.Json;

namespace NickStrupat;

public interface IDu<TDu> : IDu where TDu : IDu<TDu>
{
	public Boolean TryPick<T>(out T matched) where T : notnull;
	void Visit<T>(Action<T> action) where T : notnull;

	static virtual TDu Deserialize(ref Utf8JsonReader reader, JsonSerializerOptions options) => throw new NotImplementedException();
	void Serialize(Utf8JsonWriter writer, JsonSerializerOptions options);
}