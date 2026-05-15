using System.Text.Json;

namespace DiscriminatedUnion.Extensions;

internal static class JsonExtensions
{
    extension(JsonSerializer)
    {
        public static Boolean TryDeserialize<T>(ref Utf8JsonReader reader, JsonSerializerOptions? options, out T? value)
        {
            try
            {
                value = JsonSerializer.Deserialize<T>(ref reader, options);
                return true;
            }
            catch
            {
                value = default!;
                return false;
            }
        }
    }
}