using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace NickStrupat;

internal static class JsonExtensions
{
    public static Boolean TryDeserialize(this JsonElement element, Type type, [MaybeNullWhen(false)] out Object value, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(type);
        try
        {
            var deserialized = element.Deserialize(type, options);
            if (deserialized is not null)
            {
                value = deserialized;
                return true;
            }
            value = default!;
            return false;
        }
        catch
        {
            value = default!;
            return false;
        }
    }
    
    public static Boolean TryDeserialize<T>(this JsonElement element, [MaybeNullWhen(false)] out T value, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(element);
        try
        {
            var deserialized = element.Deserialize<T>(options);
            if (deserialized is not null)
            {
                value = deserialized;
                return true;
            }
            value = default!;
            return false;
        }
        catch
        {
            value = default!;
            return false;
        }
    }
}