using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Castle.Core.Internal;
using FluentAssertions;
using Moq;
using NickStrupat;
using ObjectLayoutInspector;

[assembly: InternalsVisibleTo(InternalsVisible.ToDynamicProxyGenAssembly2)]

namespace Tests;

// using Json = DU<JsonObject, JsonElement[], String, Int64, Double, Boolean, Null>;

public class UnitTest1
{
    [Fact]
    public void TestSize24() => TypeLayout.GetLayout<Du<Int32, String>>().Size.Should().Be(24);

    [Fact]
    public void TestStuff()
    {
        var am1 = new Mock<Action<Int32>>();
        var am2 = new Mock<Action<String>>();

        Du<Int32, String> du = 1;
        du.Switch(am1.Object, am2.Object);

        am1.Verify(x => x(It.IsAny<Int32>()), Times.Once);
        am1.Verify(x => x(1), Times.Once);
        am2.Verify(x => x(It.IsAny<String>()), Times.Never);
    }

    private static void Throw<T>(T obj) => throw new();

    [Fact]
    public void TestSwitch_WithInt()
    {
        Du<Int32?, String> du = 1;
        du.Switch(x => x.Should().Be(1), Throw);
    }

    [Fact]
    public void TestSwitch_WithString()
    {
        Du<Int32?, String> du = new("1");
        du.Switch(Throw, A2);

        static void A2(String x) => x.Should().Be("1");
    }

    [Fact]
    public void TestMatch_WithInt()
    {
        Du<Int32?, String> du = 1;
        du.Match(x => x, _ => 0).Should().Be(1);
    }

    [Fact]
    public void TestMatch_WithString()
    {
        Du<Int32?, String> du = "test";
        du.Match(Throw, A2).Should().Be("test".Length);

        static Int32? Throw<TX>(TX _) => throw new();

        static Int32? A2(String x)
        {
            x.Should().Be("test");
            return x.Length;
        }
    }

    public record Wow(Du<Int32, String> Foo, Du<Int32, String> Bar);

    [Fact]
    public void JsonTest()
    {
        var du = new Wow(new(1), new("test"));
        var json = JsonSerializer.Serialize(du);
        var du2 = JsonSerializer.Deserialize<Wow>(json)!;
        du2.Foo.Switch(x => x.Should().Be(1), Throw);
        du2.Bar.Switch(Throw, x => x.Should().Be("test"));
    }

    [Fact]
    public void JsonSimpleAndComplex()
    {
        Du<String, Foo> du = new Foo("Test");
        var json = JsonSerializer.Serialize(du);
        var du2 = JsonSerializer.Deserialize<Du<String, Foo>>(json);
        du2.Switch(Throw, x => x.Name.Should().Be("Test"));
    }

    internal class Foo(String name)
    {
        public Int32 Id { get; set; }
        public String Name { get; set; } = name;
    }

    [Fact]
    public void JsonSimpleAndArray()
    {
        Du<String, Foo[]> du = new Foo[] { new("test"), new("test2") };
        var json = JsonSerializer.Serialize(du);
        json.Should().Be("""
                         [{"Id":0,"Name":"test"},{"Id":0,"Name":"test2"}]
                         """);
        var du2 = JsonSerializer.Deserialize<Du<String, Foo[]>>(json);
        du2.Switch(Throw, x => x.Length.Should().Be(2));
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
        func.Should().Throw<JsonException>()
            .WithMessage($"No match was found for converting the JSON into a DU<{typeof(T).Name}, Int32>");
    }

    [Theory]
    [MemberData(nameof(NullableData))]
    public void NullJson_NullInTheDu<T>(Dummy<T> _) where T : notnull
    {
        var mockStringAction = new Mock<Action<T>>();
        var mockNullAction = new Mock<Action<Null>>();

        var du = JsonSerializer.Deserialize<Du<T, Null>>("null");
        du.Switch(
            mockStringAction.Object,
            mockNullAction.Object
        );

        mockStringAction.Verify(x => x(It.IsAny<T>()), Times.Never);
        mockNullAction.Verify(x => x(It.IsAny<Null>()), Times.Once);
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