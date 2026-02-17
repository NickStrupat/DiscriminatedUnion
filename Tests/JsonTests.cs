using System.Text.Json;
using AwesomeAssertions;
using Moq;
using NickStrupat;

namespace Tests;

public class JsonTests
{
	public record Wow(Du<Int32, String> Foo, Du<Int32, String> Bar);

	[Fact]
	public void JsonTest()
	{
		var du = new Wow(new(1), new("test"));

		var json = JsonSerializer.Serialize(du);
		var du2 = JsonSerializer.Deserialize<Wow>(json)!;

		{
			var am1 = new Mock<Action<Int32>>();
			var am2 = new Mock<Action<String>>();
			du2.Foo.Switch(am1.Object, am2.Object);
			am1.Verify(x => x(It.IsAny<Int32>()), Times.Once);
			am1.Verify(x => x(1), Times.Once);
			am2.Verify(x => x(It.IsAny<String>()), Times.Never);
		}

		{
			var am1 = new Mock<Action<Int32>>();
			var am2 = new Mock<Action<String>>();
			du2.Bar.Switch(am1.Object, am2.Object);
			am1.Verify(x => x(It.IsAny<Int32>()), Times.Never);
			am2.Verify(x => x(It.IsAny<String>()), Times.Once);
			am2.Verify(x => x("test"), Times.Once);
		}
	}

	[Fact]
	public void JsonSimpleAndComplex()
	{
		var am1 = new Mock<Action<String>>();
		var am2 = new Mock<Action<Foo>>();
		Du<String, Foo> du = new Foo("Test");

		var json = JsonSerializer.Serialize(du);
		var du2 = JsonSerializer.Deserialize<Du<String, Foo>>(json);

		du2.Switch(am1.Object, am2.Object);
		am1.Verify(x => x(It.IsAny<String>()), Times.Never);
        am2.Verify(x => x(It.IsAny<Foo>()), Times.Once);
        am2.Verify(x => x(It.Is<Foo>(f => f.Name == "Test")), Times.Once);
	}

	internal class Foo(String name)
	{
		public Int32 Id { get; set; }
		public String Name { get; set; } = name;
	}

	[Fact]
	public void JsonSimpleAndArray()
	{
		var am1 = new Mock<Action<String>>();
		var am2 = new Mock<Action<Foo[]>>();
		Du<String, Foo[]> du = new Foo[] { new("test"), new("test2") };

		var json = JsonSerializer.Serialize(du);

		json.Should().Be("""
		                 [{"Id":0,"Name":"test"},{"Id":0,"Name":"test2"}]
		                 """);

		var du2 = JsonSerializer.Deserialize<Du<String, Foo[]>>(json);
		du2.Switch(am1.Object, am2.Object);
        am1.Verify(x => x(It.IsAny<String>()), Times.Never);
        am2.Verify(x => x(It.IsAny<Foo[]>()), Times.Once);
        am2.Verify(x => x(It.Is<Foo[]>(arr => arr.Length == 2 && arr[0].Name == "test" && arr[1].Name == "test2")), Times.Once);
	}

	[Fact]
	public void JsonNested()
	{
		var ma1 = Mock.Of<Action<String>>();
		var ma2 = Mock.Of<Action<Int32>>();
		var ma3 = Mock.Of<Action<Foo>>();

		Du<String, Du<Int32, Foo>> du2 = new Du<Int32, Foo>(1);

		var json = JsonSerializer.Serialize(du2);
		json.Should().Be("1");

		var du3 = JsonSerializer.Deserialize<Du<String, Du<Int32, Foo>>>(json);
		du3.Switch(
			ma1,
			x => x.Switch(
				ma2,
				ma3
			)
		);

		Mock.Get(ma1).Verify(x => x(It.IsAny<String>()), Times.Never);
		Mock.Get(ma2).Verify(x => x(It.IsAny<Int32>()), Times.Once);
		Mock.Get(ma2).Verify(x => x(1), Times.Once);
		Mock.Get(ma3).Verify(x => x(It.IsAny<Foo>()), Times.Never);
	}

	[Theory]
	[MemberData(nameof(NullableData))]
	public void NullJson_NoNullInTheDu<T>(Dummy<T> _) where T : notnull
	{
		var func = () => JsonSerializer.Deserialize<Du<T, Int32>>("null");
		func.Should().Throw<JsonException>().Where(x =>
			x.Message == $"No match was found for converting the JSON into a {nameof(Du<,>)}<{typeof(T).Name}, Int32>");
	}

	[Theory]
	[MemberData(nameof(NullableData))]
	public void NullJson_NullInTheDu<T>(Dummy<T> _) where T : notnull
	{
		var mockStringAction = new Mock<Action<T>>();
		var mockNullAction = new Mock<Action<None>>();

		var du = JsonSerializer.Deserialize<Du<T, None>>("null");
		du.Switch(
			mockStringAction.Object,
			mockNullAction.Object
		);

		mockStringAction.Verify(x => x(It.IsAny<T>()), Times.Never);
		mockNullAction.Verify(x => x(It.IsAny<None>()), Times.Once);
	}

	public static IEnumerable<Object[]> NullableData =>
	[
		[new Dummy<String>()],
		[new Dummy<String?>()],
		[new Dummy<Int32>()],
		[new Dummy<Int32?>()],
	];

	public struct Dummy<T>;

	public static IEnumerable<Object[]> Data =>
	[
		["test"],
		[1],
		[new Foo("test")]
	];
}