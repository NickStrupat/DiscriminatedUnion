using AwesomeAssertions;
using DiscriminatedUnion;
using DiscriminatedUnion.Extensions;
using Moq;

namespace Tests;

public partial class DuBaseExtensionTests
{
	partial class Result : DuBase<Int32, String>;
	partial class TriResult : DuBase<Int32, String, Double>;

	[Fact]
	public void Pick_OnDuBaseSubclass_ExtractsMatchingArm()
	{
		var r = new Result(42);

		Du<String, None> residual = r.Pick(out Int32 matched);

		residual.TryPick<None>(out _).Should().BeTrue();
		matched.Should().Be(42);
	}

	[Fact]
	public void Pick_OnDuBaseSubclass_NonMatch_ReturnsResidual()
	{
		var r = new Result("hi");

		Du<String, None> residual = r.Pick(out Int32 matched);

		matched.Should().Be(0);
		residual.TryPick<String>(out var s).Should().BeTrue();
		s.Should().Be("hi");
	}

	[Fact]
	public void When_OnDuBaseSubclass_InvokesHandlerOnMatch()
	{
		var handler = new Mock<Action<String>>();
		var r = new Result("matched");

		Du<Int32, None> residual = r.When(handler.Object);

		residual.TryPick<None>(out _).Should().BeTrue();
		handler.Verify(h => h("matched"), Times.Once);
	}

	[Fact]
	public void PipeOperator_OnDuBaseSubclass_RunsChain()
	{
		var intHandler = new Mock<Action<Int32>>();
		var stringHandler = new Mock<Action<String>>();
		var doubleHandler = new Mock<Action<Double>>();
		var tri = new TriResult(3.14);

		None terminator = tri | intHandler.Object | stringHandler.Object | doubleHandler.Object;
		intHandler.Verify(h => h(It.IsAny<Int32>()), Times.Never);
		stringHandler.Verify(h => h(It.IsAny<String>()), Times.Never);
		doubleHandler.Verify(h => h(3.14), Times.Once);
	}

	[Fact]
	public void PipeOperator_OnDuBaseSubclass_ElseHandlerFires()
	{
		var intHandler = new Mock<Action<Int32>>();
		var elseHandler = new Mock<Action<Else>>();
		var tri = new TriResult("unhandled");

		None terminator = tri | intHandler.Object | elseHandler.Object;
		intHandler.Verify(h => h(It.IsAny<Int32>()), Times.Never);
		elseHandler.Verify(h => h(It.Is<Else>(e => e.Value.Equals("unhandled"))), Times.Once);
	}

	[Fact]
	public void PipeOperator_OnDuBaseSubclass_ParameterlessElseFires()
	{
		var elseHandler = new Mock<Action>();
		var r = new Result(99);

		None terminator = r | elseHandler.Object;
		elseHandler.Verify(h => h(), Times.Once);
	}

	[Fact]
	public void Else_OnDuBaseSubclass_InvokesWithBoxedValue()
	{
		var handler = new Mock<Action<Object>>();
		var r = new Result(7);

		None result = r.Else(handler.Object);
		handler.Verify(h => h(7), Times.Once);
	}

	[Fact]
	public void Chain_OnDuBaseSubclass_TransitionsToDuResidualThenContinues()
	{
		// First call hits the DuBase extension; residual is Du<X, None> or Du<Du<...>, None>, so
		// subsequent calls dispatch to the wrapper / Du-based extensions accordingly.
		var intHandler = new Mock<Action<Int32>>();
		var stringHandler = new Mock<Action<String>>();
		var tri = new TriResult(42);

		None terminator = (tri | intHandler.Object | stringHandler.Object).Else(_ => { });
		intHandler.Verify(h => h(42), Times.Once);
		stringHandler.Verify(h => h(It.IsAny<String>()), Times.Never);
	}
}
