using System.Collections.Immutable;
using System.Text.Json;
using NickStrupat;

namespace Tests;

public partial class RecursiveTests
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
		Assert.NotNull(deserialized);
		Assert.True(deserialized.TryPick<JsonObject>(out var obj));
		Assert.True(obj.TryGetValue("key2", out var array));
		Assert.True(array.TryPick<JsonValue[]>(out var values));
		Assert.NotNull(values[3]);
		Assert.True(values[3].TryPick<None>(out var @null));
	}

	[Du<JsonObject, JsonValue[], String, Int64, Double, Boolean, None>]
	partial class JsonValue;

	sealed class JsonObject : Dictionary<String, JsonValue>;

	[Fact]
	public void DeserializeNullWithoutNullInTheDu()
	{
		var json = "null";
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<NonNullable>(json));
	}

	[Fact]
	public void DeserializeNullWithNullInTheDu()
	{
		var json = "null";
		_ = JsonSerializer.Deserialize<Nullable>(json);
	}

	[Du<String, None>]
	partial class Nullable;

	[Du<String, Guid>]
	partial class NonNullable;
}