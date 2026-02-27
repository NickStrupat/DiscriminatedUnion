using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NickStrupat;

internal sealed class DuJsonConverter : JsonConverterFactory
{
    public override Boolean CanConvert(Type typeToConvert) => typeToConvert.IsAssignableTo(typeof(IDu));

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(Converter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class Converter<T> : JsonConverter<T> where T : struct, IDu<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
        {
            var visitor = new JsonDeserializerVisitor<T>(options);
            T.AcceptTypes(ref visitor, ref reader);
            return visitor.Du.Match(
                du => du,
                _ => throw new JsonException("No match was found for converting the JSON into a " + typeof(T).NameWithGenericArguments)
            );
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            value.Serialize(writer, options);
        }
    }
}