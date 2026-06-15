using System.Text.Json;
using DiscriminatedUnion;

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

	partial class JsonValue : DuBase<JsonObject, JsonValue[], String, Int64, Double, Boolean, None>;

	sealed class JsonObject : Dictionary<String, JsonValue>;

	[Fact]
	public void Deserialize_JsonNull_WithoutNoneArm_Throws()
	{
		// NonNullable has no None arm, so "null" matches no arm and must be rejected
		// with the converter's "no match" error rather than any other JsonException.
		var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<NonNullable>("null"));
		Assert.Contains("No match was found", ex.Message);
	}

	[Fact]
	public void Deserialize_JsonNull_WithNoneArm_PicksNone()
	{
		// Nullable has a None arm, so "null" deserializes onto it (not the String arm)
		// and round-trips back to "null".
		var deserialized = JsonSerializer.Deserialize<Nullable>("null");
		Assert.NotNull(deserialized);
		Assert.True(deserialized.TryPick<None>(out _));
		Assert.False(deserialized.TryPick<String>(out _));
		Assert.Equal("null", JsonSerializer.Serialize(deserialized));
	}

	partial class Nullable : DuBase<String, None>;

	partial class NonNullable : DuBase<String, Guid>;
}