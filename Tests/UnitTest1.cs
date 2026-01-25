using System.Text.Json;
using FluentAssertions;
using Moq;
using NickStrupat;
using ObjectLayoutInspector;

namespace Tests;

public class UnitTest1
{
    [Fact]
    public void TestSize24() => TypeLayout.GetLayout<DU<Int32, String>>().Size.Should().Be(24);

    [Fact]
    public void TestStuff()
    {
        DU<Int32, String> du = 1;
        du.Switch(x => x.Should().Be(1), Throw);
    }

    [Fact]
    public void TestSwitch_WithInt()
    {
        DU<Int32?, String> du = 1;
        du.Switch(A1, Throw);

        static void Throw<TX>(TX _) => throw new();
        static void A1(Int32? x) => x.Should().Be(1);
    }

    [Fact]
    public void TestSwitch_WithString()
    {
        DU<Int32?, String> du = new("1");
        du.Switch(Throw, A2);

        static void Throw<TX>(TX _) => throw new();
        static void A2(String x) => x.Should().Be("1");
    }

    [Fact]
    public void TestMatch_WithInt()
    {
        DU<Int32?, String> du = 1;
        du.Match(A1, Throw).Should().Be(1);

        static Int32? Throw<TX>(TX _) => throw new();
        static Int32? A1(Int32? x)
        {
            x.Should().Be(1);
            return x;
        }
    }

    [Fact]
    public void TestMatch_WithString()
    {
        DU<Int32?, String> du = "test";
        du.Match(Throw, A2).Should().Be("test".Length);

        static Int32? Throw<TX>(TX _) => throw new();
        static Int32? A2(String x)
        {
            x.Should().Be("test");
            return x.Length;
        }
    }

    public record Wow(DU<Int32, String> Foo, DU<Int32, String> Bar);

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
        DU<String, Foo> du = new Foo("Test");
        var json = JsonSerializer.Serialize(du);
        var du2 = JsonSerializer.Deserialize<DU<String, Foo>>(json);
        du2.Switch(Throw, x => x.Name.Should().Be("Test"));
    }

    class Foo(String name)
    {
        public Int32 Id { get; set; }
        public String Name { get; set; } = name;
    }

    [Fact]
    public void JsonSimpleAndArray()
    {
        DU<String, Foo[]> du = new Foo[] { new("test"), new("test2") };
        var json = JsonSerializer.Serialize(du);
        json.Should().Be("""
                         [{"Id":0,"Name":"test"},{"Id":0,"Name":"test2"}]
                         """);
        var du2 = JsonSerializer.Deserialize<DU<String, Foo[]>>(json);
        du2.Switch(Throw, x => x.Length.Should().Be(2));
    }

    [Fact]
    public void JsonNested()
    {
        // re-write the DU index logic to mean the left-to-right index of the type, including all nested DUs, flattened
        DU<Int32, Foo> du = 1;
        DU<String, DU<Int32, Foo>> du2 = du;
        var json = JsonSerializer.Serialize(du2);
        var du3 = JsonSerializer.Deserialize<DU<String, DU<Int32, Foo>>>(json);

        du3.Switch(
            Throw,
            x => x.Switch(
                y => y.Should().Be(1),
                Throw
            )
        );
    }

    [Fact]
    public void NullJson_NoNullInTheDu()
    {
        var func = () => JsonSerializer.Deserialize<DU<String, Int32>>("null");
        func.Should().Throw<JsonException>().WithMessage("No match was found for converting the JSON into a DU<String, Int32>");
    }

    [Fact]
    public void NullJson_NullInTheDu()
    {
        var mockAction = new Mock<Action<Null>>();

        var du = JsonSerializer.Deserialize<DU<String, Null>>("null");
        du.Switch(
            Throw,
            mockAction.Object
        );

        mockAction.Verify(x => x(It.IsAny<Null>()), Times.Once);
    }

    static void Throw<T>(T _) => throw new();

    public static IEnumerable<Object[]> Data =>
    [
        ["test"],
        [1],
        [new Foo("test")]
    ];
}