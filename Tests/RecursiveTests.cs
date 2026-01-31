using System.Collections.Immutable;
using System.Text.Json;
using NickStrupat;

namespace Tests;

public class RecursiveTests
{
	[Fact]
	public void Test()
	{
		var json = new JsonValue("test");
		var serialized = JsonSerializer.Serialize(json);
		JsonValue? deserialized = JsonSerializer.Deserialize<JsonValue>(serialized);
		Assert.NotNull(deserialized);
	}

	[Fact]
	public void Deserialize()
	{
		var json =
			"""
			{
				"key1": "value1",
				"key2": [123, 456.78, true, null, {"nestedKey": "nestedValue"}]
			}
			""";
		var deserialized = JsonSerializer.Deserialize<JsonValue>(json);
	}
}

[Du<JsonObject, JsonValue[], String, Int64, Double, Boolean, Null>]
partial class JsonValue;

sealed class JsonObject : Dictionary<String, JsonValue>;