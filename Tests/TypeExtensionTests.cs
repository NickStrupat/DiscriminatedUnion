using FluentAssertions;
using NickStrupat;

namespace Tests;

public class TypeExtensionTests
{
	[Theory]
	[InlineData(typeof(Dictionary<,>), "Dictionary<TKey, TValue>")]
	[InlineData(typeof(List<>), "List<T>")]
	[InlineData(typeof(Dictionary<Int32, String>), "Dictionary<Int32, String>")]
	[InlineData(typeof(List<Double>), "List<Double>")]
	[InlineData(typeof(String), "String")]
	public void TestNameWithGenericArguments(Type type, String expected)
	{
		type.NameWithGenericArguments.Should().Be(expected);
	}
}