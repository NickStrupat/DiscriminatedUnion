using System.Text.Json;
using AwesomeAssertions;
using NickStrupat;

namespace Tests;

public class AdvancedTests
{
	[Fact]
	public void TestEquality_SameInstance()
	{
		Du<Int32, String> du1 = 42;
		Du<Int32, String> du2 = 42;
		du1.Should().Be(du2);
	}

	[Fact]
	public void TestEquality_DifferentTypesButSameValue()
	{
		Du<Int32, String> du1 = 42;
		Du<String, Int32> du2 = 42;
		du1.Should().Be(du2);
	}

	[Fact]
	public void TestEquality_SameInt32Value()
	{
		Du<Int32, String> du1 = 42;
		du1.Should().Be(42);
	}

	[Fact]
	public void TestEquality_DifferentTypes()
	{
		Du<Int32, String> du1 = 42;
		Du<Int32, String> du2 = "42";
		du1.Should().NotBe(du2);
	}

	[Fact]
	public void TestEquality_SameStringValue()
	{
		Du<Int32, String> du1 = "test";
		Du<Int32, String> du2 = "test";
		du1.Should().Be(du2);
	}

	[Fact]
	public void TestEquality_DifferentStringValue()
	{
		Du<Int32, String> du1 = "test1";
		Du<Int32, String> du2 = "test2";
		du1.Should().NotBe(du2);
	}

	[Fact]
	public void TestGetHashCode_ConsistentForSameValue()
	{
		Du<Int32, String> du = 42;
		var hash1 = du.GetHashCode();
		var hash2 = du.GetHashCode();
		hash1.Should().Be(hash2);
	}

	[Fact]
	public void TestGetHashCode_EqualObjectsHaveSameHash()
	{
		Du<Int32, String> du1 = 42;
		Du<Int32, String> du2 = 42;
		du1.GetHashCode().Should().Be(du2.GetHashCode());
	}

	[Fact]
	public void TestGetHashCode_DifferentTypeButEqualObjectsHaveSameHash()
	{
		Du<Int32, String> du1 = 42;
		Du<String, Int32> du2 = 42;
		du1.GetHashCode().Should().Be(du2.GetHashCode());
	}

	[Fact]
	public void TestToString_Int32()
	{
		Du<Int32, String> du = 42;
		du.ToString().Should().Be("42");
	}

	[Fact]
	public void TestToString_String()
	{
		Du<Int32, String> du = "hello";
		du.ToString().Should().Be("hello");
	}

	[Fact]
	public void TestToString_Null()
	{
		Du<String, Int32> du = "null";
		du.ToString().Should().Be("null");
	}

	[Fact]
	public void TestToString_EmptyString()
	{
		Du<Int32, String> du = "";
		du.ToString().Should().Be("");
	}

	[Fact]
	public void TestTryPick_Success()
	{
		Du<Int32, String> du = 42;
		var result = du.TryPick(out Int32 value);
		result.Should().BeTrue();
		value.Should().Be(42);
	}

	[Fact]
	public void TestTryPick_Failure()
	{
		Du<Int32, String> du = "test";
		var result = du.TryPick(out Int32 value);
		result.Should().BeFalse();
		value.Should().Be(default);
	}

	[Fact]
	public void TestTryPick_MultipleTypes()
	{
		Du<Int32, String, Double> du = 3.14;
		du.TryPick(out Double value).Should().BeTrue();
		value.Should().Be(3.14);
		du.TryPick(out Int32 _).Should().BeFalse();
		du.TryPick(out String? _).Should().BeFalse();
	}

	[Fact]
	public void TestVisit_CallsCorrectAction()
	{
		var visitedInt = false;
		var visitedString = false;

		Du<Int32, String> du = 42;
		du.Visit((Int32 x) =>
		{
			visitedInt = true;
			x.Should().Be(42);
		});

		visitedInt.Should().BeTrue();
		visitedString.Should().BeFalse();
	}

	[Fact]
	public void TestVisit_NoMatchDoesNotThrow()
	{
		Du<Int32, String> du = 42;
		Action act = () => du.Visit((String _) => throw new Exception("Should not be called"));
		act.Should().NotThrow();
	}

	[Fact]
	public void TestThreeTypeVariant_SwitchAllTypes()
	{
		var int32Called = false;
		var stringCalled = false;
		var doubleCalled = false;

		Du<Int32, String, Double> du1 = 42;
		du1.Switch(
			x => int32Called = x == 42,
			_ => stringCalled = true,
			_ => doubleCalled = true
		);

		int32Called.Should().BeTrue();
		stringCalled.Should().BeFalse();
		doubleCalled.Should().BeFalse();

		int32Called = false;
		Du<Int32, String, Double> du2 = "test";
		du2.Switch(
			_ => int32Called = true,
			x => stringCalled = x == "test",
			_ => doubleCalled = true
		);

		int32Called.Should().BeFalse();
		stringCalled.Should().BeTrue();
		doubleCalled.Should().BeFalse();

		stringCalled = false;
		Du<Int32, String, Double> du3 = 3.14d;
		du3.Switch(
			_ => int32Called = true,
			_ => stringCalled = true,
			x => doubleCalled = Math.Abs(x - 3.14d) < 0.001
		);

		int32Called.Should().BeFalse();
		stringCalled.Should().BeFalse();
		doubleCalled.Should().BeTrue();
	}

	[Fact]
	public void TestThreeTypeVariant_MatchAllTypes()
	{
		Du<Int32, String, Double> du1 = 42;
		du1.Match(
			x => x.ToString(System.Globalization.CultureInfo.InvariantCulture),
			x => x,
			x => x.ToString(System.Globalization.CultureInfo.InvariantCulture)
		).Should().Be("42");

		Du<Int32, String, Double> du2 = "test";
		du2.Match(
			x => x.ToString(System.Globalization.CultureInfo.InvariantCulture),
			x => x,
			x => x.ToString(System.Globalization.CultureInfo.InvariantCulture)
		).Should().Be("test");

		Du<Int32, String, Double> du3 = 3.14d;
		var result = du3.Match(
			x => x.ToString(System.Globalization.CultureInfo.InvariantCulture),
			x => x,
			x => x.ToString(System.Globalization.CultureInfo.InvariantCulture)
		);
		result.Should().StartWith("3.14");
	}

	[Fact]
	public void TestImplicitConversion_FromInt32()
	{
		Du<Int32, String> du = 42;
		du.TryPick(out Int32 value).Should().BeTrue();
		value.Should().Be(42);
	}

	[Fact]
	public void TestImplicitConversion_FromString()
	{
		Du<Int32, String> du = "test";
		du.TryPick<String>(out var value).Should().BeTrue();
		value.Should().Be("test");
	}

	[Fact]
	public void TestJsonSerialization_ThreeTypes()
	{
		Du<Int32, String, Double> du1 = 42;
		var json1 = JsonSerializer.Serialize(du1);
		var du1Deserialized = JsonSerializer.Deserialize<Du<Int32, String, Double>>(json1);
		du1Deserialized.TryPick<Int32>(out var val1).Should().BeTrue();
		val1.Should().Be(42);

		Du<Int32, String, Double> du2 = "test";
		var json2 = JsonSerializer.Serialize(du2);
		var du2Deserialized = JsonSerializer.Deserialize<Du<Int32, String, Double>>(json2);
		du2Deserialized.TryPick<String>(out var val2).Should().BeTrue();
		val2.Should().Be("test");

		Du<Int32, String, Double> du3 = 3.14;
		var json3 = JsonSerializer.Serialize(du3);
		var du3Deserialized = JsonSerializer.Deserialize<Du<Int32, String, Double>>(json3);
		du3Deserialized.TryPick<Double>(out var val3).Should().BeTrue();
		val3.Should().Be(3.14);
	}

	[Fact]
	public void TestJsonSerialization_ComplexObject()
	{
		var obj = new TestObject { Id = 1, Name = "Test", Values = new[] { 1, 2, 3 } };
		Du<String, TestObject> du = obj;
		var json = JsonSerializer.Serialize(du);
		var deserialized = JsonSerializer.Deserialize<Du<String, TestObject>>(json);
		deserialized.TryPick<TestObject>(out var deserializedObj).Should().BeTrue();
		deserializedObj!.Id.Should().Be(1);
		deserializedObj.Name.Should().Be("Test");
		deserializedObj.Values.Should().Equal(1, 2, 3);
	}

	[Fact]
	public void TestJsonSerialization_Array()
	{
		var arr = new[] { 1, 2, 3, 4, 5 };
		Du<String, Int32[]> du = arr;
		var json = JsonSerializer.Serialize(du);
		var deserialized = JsonSerializer.Deserialize<Du<String, Int32[]>>(json);
		deserialized.TryPick<Int32[]>(out var deserializedArr).Should().BeTrue();
		deserializedArr.Should().Equal(1, 2, 3, 4, 5);
	}

	[Fact]
	public void TestJsonSerialization_NestedDu()
	{
		Du<String, Du<Int32, Double>> du = new Du<Int32, Double>(42);
		var json = JsonSerializer.Serialize(du);
		var deserialized = JsonSerializer.Deserialize<Du<String, Du<Int32, Double>>>(json);
		deserialized.TryPick<Du<Int32, Double>>(out var inner).Should().BeTrue();
		inner.TryPick<Int32>(out var value).Should().BeTrue();
		value.Should().Be(42);
	}

	[Fact]
	public void TestValueType_Struct()
	{
		var point = new Point(10, 20);
		Du<String, Point> du = point;
		du.TryPick<Point>(out var retrieved).Should().BeTrue();
		retrieved.Should().Be(point);
	}

	[Fact]
	public void TestValueType_Struct_Equality()
	{
		Du<String, Point> du1 = new Point(10, 20);
		Du<String, Point> du2 = new Point(10, 20);
		du1.Should().Be(du2);
	}

	[Fact]
	public void TestValueType_Struct_Hash()
	{
		Du<String, Point> du1 = new Point(10, 20);
		Du<String, Point> du2 = new Point(10, 20);
		du1.GetHashCode().Should().Be(du2.GetHashCode());
	}

	[Fact]
	public void TestMultipleInstances()
	{
		var dus = Enumerable.Range(0, 100)
			.Select(i => i % 2 == 0 ? (Du<Int32, String>)i : (Du<Int32, String>)i.ToString())
			.ToList();

		dus.Where(du => du.TryPick<Int32>(out _)).Should().HaveCount(50);
		dus.Where(du => du.TryPick<String>(out _)).Should().HaveCount(50);
	}

	[Fact]
	public void TestSwitchDoesNotThrowWithCorrectType()
	{
		Du<Int32, String> du = 42;
		Action act = () => du.Switch(
			x => x.Should().Be(42),
			x => throw new Exception("Should not be called")
		);
		act.Should().NotThrow();
	}

	[Fact]
	public void TestMatchReturnsCorrectValue()
	{
		Du<Int32, String> du1 = 42;
		var result1 = du1.Match(x => x * 2, x => x.Length);
		result1.Should().Be(84);

		Du<Int32, String> du2 = "hello";
		var result2 = du2.Match(x => x * 2, x => x.Length);
		result2.Should().Be(5);
	}

	[Fact]
	public void TestCombinedOperations()
	{
		var list = new List<Du<Int32, String>>
		{
			42,
			"test",
			100,
			"another",
			-5,
			"last"
		};

		var intSum = list
			.Where(du => du.TryPick<Int32>(out _))
			.Aggregate(0, (acc, du) =>
			{
				du.TryPick<Int32>(out var val);
				return acc + val;
			});

		intSum.Should().Be(137); // 42 + 100 + (-5)

		var stringConcat = list
			.Where(du => du.TryPick<String>(out _))
			.Aggregate("", (acc, du) =>
			{
				du.TryPick<String>(out var val);
				return acc + val;
			});

		stringConcat.Should().Be("testanotherlast");
	}

	[Fact]
	public void TestSwitch_WithThreeTypes_CorrectCaller()
	{
		var callers = new List<String>();

		Du<Int32, String, Double> du1 = 42;
		du1.Switch(
			_ => callers.Add("int"),
			_ => callers.Add("string"),
			_ => callers.Add("double")
		);

		Du<Int32, String, Double> du2 = "test";
		du2.Switch(
			_ => callers.Add("int"),
			_ => callers.Add("string"),
			_ => callers.Add("double")
		);

		Du<Int32, String, Double> du3 = 3.14;
		du3.Switch(
			_ => callers.Add("int"),
			_ => callers.Add("string"),
			_ => callers.Add("double")
		);

		callers.Should().Equal("int", "string", "double");
	}

	public class TestObject
	{
		public Int32 Id { get; set; }
		public String? Name { get; set; }
		public Int32[]? Values { get; set; }
	}

	public struct Point
	{
		public Int32 X { get; set; }
		public Int32 Y { get; set; }

		public Point(Int32 x, Int32 y)
		{
			X = x;
			Y = y;
		}

		public override Boolean Equals(Object? obj) =>
			obj is Point point && X == point.X && Y == point.Y;

		public override Int32 GetHashCode() =>
			HashCode.Combine(X, Y);
	}
}
