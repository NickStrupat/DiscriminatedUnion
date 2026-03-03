using System.Text.Json;
using System.Text.Json.Serialization;

namespace NickStrupat;

[JsonConverter(typeof(MaybeJsonConverterFactory))]
public readonly struct Maybe<T>(T value) : IMaybeInternal
{
	public Boolean HasValue { get; } = true; // will be false when the struct is default-initialized
	public T Value => HasValue ? value : throw new InvalidOperationException("Maybe has no value.");
	public T GetValueOrDefault(T @default = default!) => HasValue ? Value : @default;
	public static implicit operator Maybe<T>(T value) => new(value);
	public static explicit operator T(Maybe<T> maybe) => maybe.Value;
	public override String ToString() => HasValue && Value is not null && Value.ToString() is { } s ? s : String.Empty;
}

internal interface IMaybeInternal;

internal sealed class MaybeJsonConverterFactory : JsonConverterFactory
{
	public override Boolean CanConvert(Type typeToConvert) => typeToConvert.IsAssignableTo(typeof(IMaybeInternal));

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var typeArgument = typeToConvert.GenericTypeArguments.Single();
		var converterType = typeof(Converter<>).MakeGenericType(typeArgument);
		return (JsonConverter)Activator.CreateInstance(converterType)!;
	}

	private sealed class Converter<T> : JsonConverter<Maybe<T>>
	{
		public override Maybe<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return default;
			return new(JsonSerializer.Deserialize<T>(ref reader, options)!);
		}

		public override void Write(Utf8JsonWriter writer, Maybe<T> value, JsonSerializerOptions options)
		{
			if (value.HasValue)
				JsonSerializer.Serialize(writer, value.Value, options);
			else
				writer.WriteNullValue();
		}
	}
}