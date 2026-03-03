using System.Text.Json;
using AwesomeAssertions;
using NickStrupat;

namespace Tests;

public class MaybeTests
{
	[Fact]
	public void DefaultInitialized()
	{
		Maybe<Int32> maybe = default;
		maybe.HasValue.Should().BeFalse();
		maybe.GetValueOrDefault().Should().Be(0);
		maybe.GetValueOrDefault(1).Should().Be(1);
		var act = () => maybe.Value;
		act.Should().Throw<InvalidOperationException>();
		var act2 = () => (Int32)maybe;
		act2.Should().Throw<InvalidOperationException>();
	}

	[Fact]
	public void InitializedWithNoValue()
	{
		Maybe<Int32> maybe = new();
		maybe.HasValue.Should().BeFalse();
		maybe.GetValueOrDefault().Should().Be(0);
		maybe.GetValueOrDefault(1).Should().Be(1);
		var act = () => maybe.Value;
		act.Should().Throw<InvalidOperationException>();
		var act2 = () => (Int32)maybe;
		act2.Should().Throw<InvalidOperationException>();
	}

	[Fact]
	public void InitializedWithValue()
	{
		Maybe<Int32> maybe = 42;
		maybe.HasValue.Should().BeTrue();
		maybe.Value.Should().Be(42);
		maybe.GetValueOrDefault().Should().Be(42);
		maybe.GetValueOrDefault(1).Should().Be(42);
		((Int32)maybe).Should().Be(42);
	}

	[Fact]
	public void JsonSerialization()
	{
		var options = new JsonSerializerOptions { WriteIndented = false };
		Maybe<Int32> maybe = 42;
		var json = JsonSerializer.Serialize(maybe, options);
		json.Should().Be("42");
		var deserialized = JsonSerializer.Deserialize<Maybe<Int32>>(json, options);
		deserialized.HasValue.Should().BeTrue();
		deserialized.Value.Should().Be(42);

		json = JsonSerializer.Serialize(default(Maybe<Int32>), options);
		json.Should().Be("null");
		deserialized = JsonSerializer.Deserialize<Maybe<Int32>>(json, options);
		deserialized.HasValue.Should().BeFalse();

		json = JsonSerializer.Serialize(new Maybe<Int32>(), options);
		json.Should().Be("null");
		deserialized = JsonSerializer.Deserialize<Maybe<Int32>>(json, options);
		deserialized.HasValue.Should().BeFalse();
		//
		// json = JsonSerializer.Serialize((Maybe<Int32>)null!, options);
		// json.Should().Be("0");
		// deserialized = JsonSerializer.Deserialize<Maybe<Int32>>(json, options);
		// deserialized.HasValue.Should().BeFalse();

		json = "null";
		deserialized = JsonSerializer.Deserialize<Maybe<Int32>>(json, options);
		deserialized.HasValue.Should().BeFalse();

		json = "\"not a number\"";
		var act = () => JsonSerializer.Deserialize<Maybe<Int32>>(json, options);
		act.Should().Throw<JsonException>();
	}
}