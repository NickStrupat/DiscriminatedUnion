using AwesomeAssertions;
using DiscriminatedUnion;
using DiscriminatedUnion.Extensions;
using Moq;

namespace Tests;

public class PipeTests
{
	[Fact]
	public void Pipe_TwoArm_MatchingArm_InvokesHandlerAndReturnsHandled()
	{
		var handler = new Mock<Action<Int32>>();
		Du<Int32, String> du = 42;

		Du<String, None> residual = du | handler.Object;

		residual.TryPick<None>(out _).Should().BeTrue();
		handler.Verify(h => h(42), Times.Once);
	}

	[Fact]
	public void Pipe_TwoArm_NonMatchingArm_DoesNotInvokeHandler_ReturnsResidual()
	{
		var handler = new Mock<Action<Int32>>();
		Du<Int32, String> du = "hello";

		Du<String, None> residual = du | handler.Object;

		residual.TryPick<String>(out var s).Should().BeTrue();
		s.Should().Be("hello");
		handler.Verify(h => h(It.IsAny<Int32>()), Times.Never);
	}

	[Fact]
	public void Pipe_ThreeArm_PickMiddle_HandlerInvokedOnMatch()
	{
		var handler = new Mock<Action<String>>();
		Du<Int32, String, Double> du = "match";

		Du<Du<Int32, Double>, None> residual = du | handler.Object;

		residual.TryPick<None>(out _).Should().BeTrue();
		handler.Verify(h => h("match"), Times.Once);
	}

	[Fact]
	public void Pipe_FluentChain_RunsExactlyOneHandlerThenTerminates()
	{
		var intHandler = new Mock<Action<Int32>>();
		var stringHandler = new Mock<Action<String>>();
		var doubleHandler = new Mock<Action<Double>>();

		Du<Int32, String, Double> du = "hit";

		None terminator = du | intHandler.Object | stringHandler.Object | doubleHandler.Object;
		intHandler.Verify(h => h(It.IsAny<Int32>()), Times.Never);
		stringHandler.Verify(h => h("hit"), Times.Once);
		doubleHandler.Verify(h => h(It.IsAny<Double>()), Times.Never);
	}

	[Fact]
	public void Pipe_FluentChain_LastArmMatches_OnlyLastHandlerInvoked()
	{
		var intHandler = new Mock<Action<Int32>>();
		var stringHandler = new Mock<Action<String>>();
		var doubleHandler = new Mock<Action<Double>>();

		Du<Int32, String, Double> du = 3.14;

		None terminator = du
		                            | intHandler.Object
		                            | stringHandler.Object
		                            | doubleHandler.Object;
		intHandler.Verify(h => h(It.IsAny<Int32>()), Times.Never);
		stringHandler.Verify(h => h(It.IsAny<String>()), Times.Never);
		doubleHandler.Verify(h => h(3.14), Times.Once);
	}

	[Fact]
	public void Pipe_FluentChain_FirstArmMatches_ShortCircuitsRemainingHandlers()
	{
		var intHandler = new Mock<Action<Int32>>();
		var stringHandler = new Mock<Action<String>>();
		var doubleHandler = new Mock<Action<Double>>();

		Du<Int32, String, Double> du = 42;

		None terminator = du | intHandler.Object | stringHandler.Object | doubleHandler.Object;
		intHandler.Verify(h => h(42), Times.Once);
		stringHandler.Verify(h => h(It.IsAny<String>()), Times.Never);
		doubleHandler.Verify(h => h(It.IsAny<Double>()), Times.Never);
	}

	[Fact]
	public void Pipe_DuWithNonePadding_PickingValueArm_TerminatesWithNoneResidual()
	{
		var handler = new Mock<Action<Int32>>();
		Du<Int32, None> du = 42;

		None residual = du | handler.Object;
		handler.Verify(h => h(42), Times.Once);
	}

	[Fact]
	public void Pipe_DuWithNonePadding_HoldingNone_DoesNotInvokeHandler()
	{
		var handler = new Mock<Action<String>>();
		Du<String, None> du = default(None);

		None residual = du | handler.Object;

		// Residual at terminator (None) has both arms as None type, so TryPick<None>
		// always returns true. The behavioral assertion is that the handler did not fire.
		handler.Verify(h => h(It.IsAny<String>()), Times.Never);
	}

	[Fact]
	public void Pipe_OnDefaultDu_Throws()
	{
		Du<Int32, String> du = default;

		Action act = () => { var _ = du | ((Int32 _) => { }); };

		act.Should().Throw<InvalidInstanceException>();
	}

	[Fact]
	public void Pipe_ElseHandler_OnDirectDu_InvokesWithBoxedValue()
	{
		var handler = new Mock<Action<Else>>();
		Du<Int32, String> du = 42;

		None result = du | handler.Object;
		handler.Verify(h => h(It.Is<Else>(r => r.Value.Equals(42))), Times.Once);
	}

	[Fact]
	public void Pipe_ElseHandler_TerminatesChain_NotInvokedAfterPriorMatch()
	{
		var intHandler = new Mock<Action<Int32>>();
		var restHandler = new Mock<Action<Else>>();
		Du<Int32, String, Double> du = 42;

		None terminator = du | intHandler.Object | restHandler.Object;
		intHandler.Verify(h => h(42), Times.Once);
		restHandler.Verify(h => h(It.IsAny<Else>()), Times.Never);
	}

	[Fact]
	public void Pipe_ElseHandler_TerminatesChain_FiresWithBoxedUnhandledValue()
	{
		var intHandler = new Mock<Action<Int32>>();
		var stringHandler = new Mock<Action<String>>();
		var restHandler = new Mock<Action<Else>>();
		Du<Int32, String, Double> du = 3.14;

		None terminator = du | intHandler.Object | stringHandler.Object | restHandler.Object;
		intHandler.Verify(h => h(It.IsAny<Int32>()), Times.Never);
		stringHandler.Verify(h => h(It.IsAny<String>()), Times.Never);
		restHandler.Verify(h => h(It.Is<Else>(r => r.Value.Equals(3.14))), Times.Once);
	}

	[Fact]
	public void Pipe_ParameterlessHandler_OnDirectDu_Invokes()
	{
		var handler = new Mock<Action>();
		Du<Int32, String> du = 42;

		None result = du | handler.Object;
		handler.Verify(h => h(), Times.Once);
	}

	[Fact]
	public void Pipe_ParameterlessHandler_TerminatesChain_NotInvokedAfterPriorMatch()
	{
		var intHandler = new Mock<Action<Int32>>();
		var elseHandler = new Mock<Action>();
		Du<Int32, String> du = 42;

		None terminator = du | intHandler.Object | elseHandler.Object;
		intHandler.Verify(h => h(42), Times.Once);
		elseHandler.Verify(h => h(), Times.Never);
	}

	[Fact]
	public void Pipe_ParameterlessHandler_TerminatesChain_FiresWhenNothingMatched()
	{
		var intHandler = new Mock<Action<Int32>>();
		var stringHandler = new Mock<Action<String>>();
		var elseHandler = new Mock<Action>();
		Du<Int32, String, Double> du = 3.14;

		None terminator = du | intHandler.Object | stringHandler.Object | elseHandler.Object;
		intHandler.Verify(h => h(It.IsAny<Int32>()), Times.Never);
		stringHandler.Verify(h => h(It.IsAny<String>()), Times.Never);
		elseHandler.Verify(h => h(), Times.Once);
	}

	[Fact]
	public void Pipe_ElseHandler_DuWithNonePadding_SkipsNoneArm()
	{
		var handler = new Mock<Action<Else>>();
		Du<String, None> du = default(None);

		None result = du | handler.Object;
		handler.Verify(h => h(It.IsAny<Else>()), Times.Never);
	}

	[Fact]
	public void Pipe_ParameterlessHandler_DuWithNonePadding_SkipsNoneArm()
	{
		var handler = new Mock<Action>();
		Du<String, None> du = default(None);

		None result = du | handler.Object;
		handler.Verify(h => h(), Times.Never);
	}
}
