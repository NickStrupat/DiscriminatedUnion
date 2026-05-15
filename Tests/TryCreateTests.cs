using AwesomeAssertions;
using NickStrupat;

namespace Tests;

public class TryCreateTests
{
	private class Animal;
	private class Dog : Animal;

	private interface IShape;
	private interface IDrawable;
	private class Circle : IShape, IDrawable;

	[Fact]
	public void TryCreate_DerivedAgainstBaseArm_Succeeds()
	{
		var dog = new Dog();
		Du<Animal, Int32>.TryCreate(dog, out var du).Should().BeTrue();
		du.TryPick<Animal>(out var stored).Should().BeTrue();
		stored.Should().BeSameAs(dog);
	}

	[Fact]
	public void TryCreate_DerivedAgainstBaseArm_StoresAsBaseArm()
	{
		var dog = new Dog();
		Du<Animal, Int32>.TryCreate(dog, out var du).Should().BeTrue();
		du.TryPick<Dog>(out _).Should().BeFalse();
	}

	[Fact]
	public void TryCreate_InterfaceArms_LeftmostWins()
	{
		var circle = new Circle();
		Du<IShape, IDrawable>.TryCreate(circle, out var du).Should().BeTrue();
		du.TryPick<IShape>(out var asShape).Should().BeTrue();
		asShape.Should().BeSameAs(circle);
		du.TryPick<IDrawable>(out _).Should().BeFalse();
	}

	[Fact]
	public void TryCreate_ExactMatchWinsOverAssignability()
	{
		// String would match the Object arm via assignability, but exact-typeof must win.
		Du<Object, String>.TryCreate("hi", out var du).Should().BeTrue();
		du.TryPick<String>(out var picked).Should().BeTrue();
		picked.Should().Be("hi");
		du.TryPick<Object>(out _).Should().BeFalse();
	}

	[Fact]
	public void TryCreate_GenericObjectAgainstObjectArm_PicksLeftmost()
	{
		Object value = "hi";
		Du<Object, String>.TryCreate(value, out var du).Should().BeTrue();
		du.TryPick<Object>(out var picked).Should().BeTrue();
		picked.Should().Be("hi");
	}

	[Fact]
	public void TryCreate_NullableValueWithHasValue_UnboxesToValueArm()
	{
		Int32? value = 42;
		Du<Int32, String>.TryCreate(value, out var du).Should().BeTrue();
		du.TryPick<Int32>(out var picked).Should().BeTrue();
		picked.Should().Be(42);
	}

	[Fact]
	public void TryCreate_NullableValueWithNoValue_ReturnsFalse()
	{
		Int32? value = null;
		Du<Int32, String>.TryCreate(value, out var du).Should().BeFalse();
	}

	[Fact]
	public void TryCreate_NoAssignableArm_ReturnsFalse()
	{
		Du<Int32, String>.TryCreate(true, out var du).Should().BeFalse();
	}

	[Fact]
	public void TryCreate_BoxedValueAgainstValueArm_Unboxes()
	{
		Object boxed = 42;
		Du<Int32, String>.TryCreate(boxed, out var du).Should().BeTrue();
		du.TryPick<Int32>(out var picked).Should().BeTrue();
		picked.Should().Be(42);
	}

	[Fact]
	public void TryCreate_NullReferenceAgainstReferenceArm_ReturnsFalse()
	{
		String? value = null;
		Du<Int32, String>.TryCreate(value, out var du).Should().BeFalse();
	}

	[Fact]
	public void TryCreate_NullObjectAgainstReferenceArms_ReturnsFalse()
	{
		Object? value = null;
		Du<Object, String>.TryCreate(value, out var du).Should().BeFalse();
	}

	[Fact]
	public void Create_DerivedAgainstBaseArm_Succeeds()
	{
		var dog = new Dog();
		var du = CreateVia<Du<Animal, Int32>, Dog>(dog);
		du.TryPick<Animal>(out var stored).Should().BeTrue();
		stored.Should().BeSameAs(dog);
	}

	[Fact]
	public void Create_NoAssignableArm_Throws()
	{
		Action act = () => CreateVia<Du<Int32, String>, Boolean>(true);
		act.Should().Throw<InvalidOperationException>();
	}

	private static TDu CreateVia<TDu, T>(T value) where TDu : IDu<TDu> => TDu.Create(value);
}
