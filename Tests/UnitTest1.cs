using System.Text.Json;
using FluentAssertions;
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
        du.Switch(x => x.Should().Be(1), _ => throw new());
    }

    // [Theory]
    // [InlineData(1)]
    // [InlineData("1")]
    // public void TestSwitch<T>(T value)
    // {
    //     DU<Int32?, String> du = new(value);
    //     if (typeof(T) == typeof(Int32))
    //         du.Switch(A1, Throw);
    //     else if (typeof(T) == typeof(String))
    //         du.Switch(Throw, A2);
    //     else
    //         throw new();
    //     return;
    //
    //     static void Throw<TX>(TX _) => throw new();
    //     static void A1(Int32? x) => x.Should().Be(1);
    //     static void A2(String x) => x.Should().Be("1");
    // }
    //
    // [Theory]
    // [InlineData(1)]
    // [InlineData("test")]
    // public void TestMatch<T>(T value)
    // {
    //     DU<Int32?, String> du = new(value);
    //     if (typeof(T) == typeof(Int32))
    //         du.Match(A1, Throw).Should().Be(1);
    //     else if (typeof(T) == typeof(String))
    //         du.Match(Throw, A2).Should().Be(value is String s ? s.Length : throw new());
    //     else
    //         throw new();
    //     return;
    //
    //     static Int32? Throw<TX>(TX s) => throw new();
    //     static Int32? A1(Int32? x)
    //     {
    //         x.Should().Be(1);
    //         return x;
    //     }
    //
    //     static Int32? A2(String x)
    //     {
    //         x.Should().Be("test");
    //         return x.Length;
    //     }
    // }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Asdf(int a)
    {
        DU<Int32, String> du = a == 1 ? 1 : "1";
        if (a == 1)
            du.Switch(x => x.Should().Be(1), _ => throw new());
        else
            du.Switch(_ => throw new(), x => x.Should().Be("1"));

    }

    public record Wow(DU<Int32, String> Foo, DU<Int32, String> Bar);

    [Fact]
    public void JsonTest()
    {
        var du = new Wow(new(1), new("test"));
        var json = JsonSerializer.Serialize(du);
        var du2 = JsonSerializer.Deserialize<Wow>(json)!;
        du2.Foo.Switch(x => x.Should().Be(1), _ => throw new());
        du2.Bar.Switch(_ => throw new(), x => x.Should().Be("test"));
    }

    [Fact]
    public void JsonSimpleAndComplex()
    {
        DU<String, Foo> du = new Foo("Test");
        var json = JsonSerializer.Serialize(du);
        var du2 = JsonSerializer.Deserialize<DU<String, Foo>>(json);
        du2.Switch(x => throw new(), x => x.Name.Should().Be("Test"));
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
        du2.Switch(x => throw new(), x => x.Length.Should().Be(2));
    }

    [Fact]
    public void JsonNested()
    {
        // re-write the DU index logic to mean the left-to-right index of the type, including all nested DUs, flattened
        DU<Int32, Foo> du = 1;
        DU<String, DU<Int32, Foo>> du2 = du;
        var json = JsonSerializer.Serialize(du2);
        var du3 = JsonSerializer.Deserialize<DU<String, DU<Int32, Foo>>>(json);
        du3.Switch(_ => throw new(), x => x.Switch(y => y.Should().Be(1), _ => throw new()));
    }

    public static IEnumerable<Object[]> Data => new List<Object[]>
    {
        new Object[] { "test" },
        new Object[] { 1 },
        new Object[] { new Foo("test") }
    };
}