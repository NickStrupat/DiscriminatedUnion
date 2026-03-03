using System.Text.Json;

namespace NickStrupat;

internal struct JsonDeserializerVisitor<TDu>(JsonSerializerOptions options) : ITypeVisitor<Utf8JsonReader>
where TDu : IDu<TDu>
{
	public Maybe<TDu> Du { get; private set; }

	public Boolean VisitType<T>(ref Utf8JsonReader reader) where T : notnull
	{
		if (!JsonSerializer.TryDeserialize<T>(ref reader, options, out var value) || value is null)
			return false;
		Du = TDu.Create(value);
		return true;
	}
}