using AwesomeAssertions;
using DiscriminatedUnion;
using DiscriminatedUnion.Extensions;
using Moq;

namespace Tests;

public class WhenTests
{
	[Fact]
	public void When_TwoArm_MatchingArm_InvokesHandlerAndReturnsHandled()
	{
		var handler = new Mock<Action<Int32>>();
		Du<Int32, String> du = 42;

		Du<String, None> residual = du.When(handler.Object);

		residual.TryPick<None>(out _).Should().BeTrue();
		handler.Verify(h => h(42), Times.Once);
	}

	[Fact]
	public void When_TwoArm_NonMatchingArm_DoesNotInvokeHandler_ReturnsResidual()
	{
		var handler = new Mock<Action<Int32>>();
		Du<Int32, String> du = "hello";

		Du<String, None> residual = du.When(handler.Object);

		residual.TryPick<String>(out var s).Should().BeTrue();
		s.Should().Be("hello");
		handler.Verify(h => h(It.IsAny<Int32>()), Times.Never);
	}

	[Fact]
	public void When_ThreeArm_PickMiddle_HandlerInvokedOnMatch()
	{
		var handler = new Mock<Action<String>>();
		Du<Int32, String, Double> du = "match";

		Du<Du<Int32, Double>, None> residual = du.When(handler.Object);

		residual.TryPick<None>(out _).Should().BeTrue();
		handler.Verify(h => h("match"), Times.Once);
	}

	[Fact]
	public void When_ThreeArm_PickFirst_NonMatch_ResidualIsTailArms()
	{
		var handler = new Mock<Action<Int32>>();
		Du<Int32, String, Double> du = 3.14;

		Du<Du<String, Double>, None> residual = du.When(handler.Object);

		residual.TryPick<Du<String, Double>>(out var inner).Should().BeTrue();
		inner.TryPick<Double>(out var d).Should().BeTrue();
		d.Should().Be(3.14);
		handler.Verify(h => h(It.IsAny<Int32>()), Times.Never);
	}

	[Fact]
	public void When_FluentChain_RunsExactlyOneHandlerThenTerminates()
	{
		var intHandler = new Mock<Action<Int32>>();
		var stringHandler = new Mock<Action<String>>();
		var doubleHandler = new Mock<Action<Double>>();

		Du<Int32, String, Double> du = "hit";

		None terminator = du
			.When(intHandler.Object)
			.When(stringHandler.Object)
			.When(doubleHandler.Object);
		intHandler.Verify(h => h(It.IsAny<Int32>()), Times.Never);
		stringHandler.Verify(h => h("hit"), Times.Once);
		doubleHandler.Verify(h => h(It.IsAny<Double>()), Times.Never);
	}

	[Fact]
	public void When_FluentChain_LastArmMatches_OnlyLastHandlerInvoked()
	{
		var intHandler = new Mock<Action<Int32>>();
		var stringHandler = new Mock<Action<String>>();
		var doubleHandler = new Mock<Action<Double>>();

		Du<Int32, String, Double> du = 3.14;

		None terminator = du
			.When(intHandler.Object)
			.When(stringHandler.Object)
			.When(doubleHandler.Object);
		intHandler.Verify(h => h(It.IsAny<Int32>()), Times.Never);
		stringHandler.Verify(h => h(It.IsAny<String>()), Times.Never);
		doubleHandler.Verify(h => h(3.14), Times.Once);
	}

	[Fact]
	public void When_DuWithNonePadding_PickingValueArm_TerminatesWithNoneResidual()
	{
		var handler = new Mock<Action<Int32>>();
		Du<Int32, None> du = 42;

		None residual = du.When(handler.Object);
		handler.Verify(h => h(42), Times.Once);
	}

	[Fact]
	public void When_DuWithNonePadding_HoldingNone_DoesNotInvokeHandler()
	{
		var handler = new Mock<Action<String>>();
		Du<String, None> du = default(None);

		None residual = du.When(handler.Object);

		// None can't structurally distinguish handled/unhandled; behavioral assertion only.
		handler.Verify(h => h(It.IsAny<String>()), Times.Never);
	}

	[Fact]
	public void When_OnDefaultDu_Throws()
	{
		Du<Int32, String> du = default;

		Action act = () => du.When((Int32 _) => { });

		act.Should().Throw<InvalidInstanceException>();
	}

	[Fact]
	public void When_MixedWithPick_Composes()
	{
		// Pick and When are interchangeable in a chain — both return the same residual shape.
		var handler = new Mock<Action<Double>>();
		Du<Int32, String, Double> du = 3.14;

		None terminator = du
			.Pick(out Int32 _)
			.Pick(out String? _)
			.When(handler.Object);
		handler.Verify(h => h(3.14), Times.Once);
	}
}
