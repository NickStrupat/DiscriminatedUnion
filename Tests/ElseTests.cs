using AwesomeAssertions;
using DiscriminatedUnion;
using DiscriminatedUnion.Extensions;
using Moq;

namespace Tests;

public class ElseTests
{
	[Fact]
	public void Else_OnDu_InvokesHandlerWithBoxedHeldValue_ReturnsNull()
	{
		var handler = new Mock<Action<Object>>();
		Du<Int32, String> du = 42;

		None? result = du.Else(handler.Object);

		result.Should().BeNull();
		handler.Verify(h => h(42), Times.Once);
	}

	[Fact]
	public void Else_OnDu_ReferenceArm_InvokesHandlerWithReference()
	{
		var handler = new Mock<Action<Object>>();
		Du<Int32, String> du = "hi";

		None? result = du.Else(handler.Object);

		result.Should().BeNull();
		handler.Verify(h => h("hi"), Times.Once);
	}

	[Fact]
	public void Else_OnNullableNull_DoesNotInvoke_ReturnsNull()
	{
		var handler = new Mock<Action<Object>>();
		Du<Int32, String>? du = null;

		None? result = du.Else(handler.Object);

		result.Should().BeNull();
		handler.Verify(h => h(It.IsAny<Object>()), Times.Never);
	}

	[Fact]
	public void Else_OnNullableNonNull_Invokes()
	{
		var handler = new Mock<Action<Object>>();
		Du<Int32, String>? du = new Du<Int32, String>(7);

		None? result = du.Else(handler.Object);

		result.Should().BeNull();
		handler.Verify(h => h(7), Times.Once);
	}

	[Fact]
	public void Else_TerminatesChain_FiresOnlyWhenNothingMatched()
	{
		var intHandler = new Mock<Action<Int32>>();
		var stringHandler = new Mock<Action<String>>();
		var elseHandler = new Mock<Action<Object>>();

		Du<Int32, String, Double> du = 3.14;

		None? terminator = (du | intHandler.Object | stringHandler.Object).Else(elseHandler.Object);

		terminator.Should().BeNull();
		intHandler.Verify(h => h(It.IsAny<Int32>()), Times.Never);
		stringHandler.Verify(h => h(It.IsAny<String>()), Times.Never);
		elseHandler.Verify(h => h(3.14), Times.Once);
	}

	[Fact]
	public void Else_TerminatesChain_DoesNotFireWhenPriorHandlerMatched()
	{
		var intHandler = new Mock<Action<Int32>>();
		var stringHandler = new Mock<Action<String>>();
		var elseHandler = new Mock<Action<Object>>();

		Du<Int32, String, Double> du = "hit";

		None? terminator = (du | intHandler.Object | stringHandler.Object).Else(elseHandler.Object);

		terminator.Should().BeNull();
		intHandler.Verify(h => h(It.IsAny<Int32>()), Times.Never);
		stringHandler.Verify(h => h("hit"), Times.Once);
		elseHandler.Verify(h => h(It.IsAny<Object>()), Times.Never);
	}

	[Fact]
	public void Else_OnDefaultDu_Throws()
	{
		Du<Int32, String> du = default;

		Action act = () => du.Else(_ => { });

		act.Should().Throw<InvalidInstanceException>();
	}
}
