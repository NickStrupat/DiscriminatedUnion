using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NickStrupat;

internal sealed class DuConverter : JsonConverterFactory
{
    public override Boolean CanConvert(Type typeToConvert) => typeToConvert.IsAssignableTo(typeof(IDu));

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        cache.GetOrAdd(typeToConvert, CreateConverter);

    private static JsonConverter CreateConverter(Type duType)
    {
        var converterType = typeof(Converter<>).MakeGenericType(duType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private static readonly ConcurrentDictionary<Type, JsonConverter> cache = new();

    private sealed class Converter<T> : JsonConverter<T> where T : IDu<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options) =>
            T.TryDeserialize(ref reader, options);

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            value.Serialize(writer, options);
    }
}