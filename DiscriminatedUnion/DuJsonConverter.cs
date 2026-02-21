using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NickStrupat;

internal sealed class DuJsonConverter : JsonConverterFactory
{
    public override Boolean CanConvert(Type typeToConvert) => typeToConvert.IsAssignableTo(typeof(IDu));

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return cache.GetOrAdd(typeToConvert, Create);

        static JsonConverter Create(Type duType)
        {
            var converterType = typeof(Converter<>).MakeGenericType(duType);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
    }

    private static readonly ConcurrentDictionary<Type, JsonConverter> cache = new();

    private sealed class Converter<T> : JsonConverter<T> where T : struct, IDu<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
        {
            var visitor = new JsonDeserializerVisitor<T>(options);
            if (!T.AcceptTypes(ref visitor, ref reader))
                throw new JsonException("No match was found for converting the JSON into a " + typeof(T).NameWithGenericArguments);
            return visitor.Du;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            value.Serialize(writer, options);
        }
    }
}