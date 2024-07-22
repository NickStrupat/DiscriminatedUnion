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
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var element = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            Object? value = element.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.Number => GetNumber(),
                JsonValueKind.String => element.GetString(),
                JsonValueKind.False => false,
                JsonValueKind.True => true,
                JsonValueKind.Object => GetObject(),
                JsonValueKind.Array => GetObject(),
                _ => throw new ArgumentOutOfRangeException()
            };
            return T.Create(value);

            Object GetNumber()
            {
                if (element.TryGetSByte(out var sb))
                    return sb;
                if (element.TryGetByte(out var b))
                    return b;
                if (element.TryGetInt16(out var s))
                    return s;
                if (element.TryGetUInt16(out var us))
                    return us;
                if (element.TryGetInt32(out var i))
                    return i;
                if (element.TryGetUInt32(out var ui))
                    return ui;
                if (element.TryGetInt64(out var l))
                    return l;
                if (element.TryGetUInt64(out var ul))
                    return ul;
                if (element.TryGetSingle(out var f))
                    return f;
                return element.GetDouble();
            }

            Object GetObject()
            {
                foreach (var type in T.Types)
                    if (element.TryDeserialize(type, out var deserialized, options))
                        return deserialized;
                throw new("No compatible type found.");
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            value.Visit(new Visitor(writer, options));
        }
    }
        
    private readonly struct Visitor(Utf8JsonWriter writer, JsonSerializerOptions options) : IVisitor
    {
        public void Visit<T>(T value) => JsonSerializer.Serialize(writer, value, options);
    }
}