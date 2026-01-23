using System.Text.Json;
using System.Text.Json.Serialization;

namespace NickStrupat;

public sealed class DUConverter : JsonConverterFactory
{
    public override Boolean CanConvert(Type typeToConvert) => typeToConvert.IsAssignableTo(typeof(IDU));

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(Converter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class Converter<T> : JsonConverter<T> where T : IDU<T>
    {
        // private readonly ref struct JsonReaderVisitor(ReadOnlySpan<Byte> utf8Json, JsonSerializerOptions options) : ITypeVisitor
        // {
        //     private readonly ReadOnlySpan<Byte> utf8Json = utf8Json;
        //     Boolean ITypeVisitor.Visit<T1>(out T1? value) where T1 : default => JsonSerializer.TryDeserialize(utf8Json, options, out value);
        // }

        private readonly struct JsonWriterVisitor(Utf8JsonWriter writer, JsonSerializerOptions options) : IVisitor
        {
            void IVisitor.Visit<TVisited>(TVisited value) => JsonSerializer.Serialize(writer, value, options);
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return T.TryDeserialize(ref reader, options);
            // var visitor = new JsonReaderVisitor(reader.ValueSpan, options);
            // return T.VisitTypes(visitor);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            value.Visit(new JsonWriterVisitor(writer, options));
        }
    }
}