using System.Text.Json;

namespace NickStrupat;

internal struct JsonDeserializerVisitor<TDu>(JsonSerializerOptions options) : ITypeVisitor<TDu, Utf8JsonReader>
where TDu : IDu<TDu>
{
	public TDu? Du { get; private set; }

	public Boolean VisitType<T>(ref Utf8JsonReader reader, Func<T, TDu> duFunc) where T : notnull
	{
		if (!JsonSerializer.TryDeserialize<T>(ref reader, options, out var value) || value is null)
			return false;
		Du = duFunc(value);
		return true;
	}
}