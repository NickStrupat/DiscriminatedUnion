using AwesomeAssertions;
using DiscriminatedUnion;
using DiscriminatedUnion.Extensions;
using Moq;

namespace Tests;

public class WhenTests
{
	[Fact]
	public void Arity2_When_RunsHandler_AndReturnsRestWithNone()
	{
		Du<Int32, String> du = 42;
		var seen = 0;

		Du<String, None> rest = du.When((Int32 i) => { seen = i; });

		seen.Should().Be(42);
		rest.TryPick<None>(out _).Should().BeTrue();
	}

	[Fact]
	public void Arity2_When_DoesNotMatch_ReturnsRestPreservingValue()
	{
		Du<Int32, String> du = "hello";
		var ran = false;

		Du<String, None> rest = du.When((Int32 _) => { ran = true; });

		ran.Should().BeFalse();
		rest.TryPick<String>(out var s).Should().BeTrue();
		s.Should().Be("hello");
	}

	[Fact]
	public void Arity2_When_OtherArm_ReturnsCorrectShape()
	{
		Du<Int32, String> du = 7;
		var ran = false;

		Du<Int32, None> rest = du.When((String _) => { ran = true; });

		ran.Should().BeFalse();
		rest.TryPick<Int32>(out var i).Should().BeTrue();
		i.Should().Be(7);
	}

	[Fact]
	public void Arity2_When_Throws_OnDefault()
	{
		Du<Int32, String> du = default;
		var act = () => du.When((Int32 _) => { });
		act.Should().Throw<InvalidInstanceException>();
	}

	[Fact]
	public void Terminal_When_NoOp_WhenChainAlreadyMatched()
	{
		Du<Int32, String> du = 42;
		var seen = 0;

		du.When((Int32 _) => { })
		  .When((String s) => { seen = 1; });

		seen.Should().Be(0);
	}

	[Fact]
	public void Terminal_When_Fires_WhenChainSurvivesToTerminal()
	{
		Du<Int32, String> du = "hi";
		var seen = "";

		du.When((Int32 _) => { })
		  .When((String s) => { seen = s; });

		seen.Should().Be("hi");
	}

	[Fact]
	public void Terminal_When_Throws_OnDefault()
	{
		Du<Int32, None> du = default;
		var act = () => du.When((Int32 _) => { });
		act.Should().Throw<InvalidInstanceException>();
	}

	[Fact]
	public void Terminal_Overload_WinsOverGeneric_ForDuOfTAndNone()
	{
		Du<Int32, None> du = 5;
		var ran = false;

		du.When((Int32 i) => { ran = i == 5; });

		ran.Should().BeTrue();
	}

	[Theory]
	[InlineData(1)]
	[InlineData("text")]
	[InlineData(true)]
	public void Arity3_When_To_Arity2_Then_Terminal_FiresExactlyOnce<T>(T value)
	{
		Assert.True(Du<Int32, String, Boolean>.TryCreate(value, out var du));
		var fm1 = new Mock<Action<Int32>>();
		var fm2 = new Mock<Action<String>>();
		var fm3 = new Mock<Action<Boolean>>();

		du
			.When(fm1.Object)
			.When(fm2.Object)
			.When(fm3.Object);

		if (typeof(T) ==  typeof(Int32))
			fm1.Verify(x => x(It.IsAny<Int32>()), Times.Once);
		else if (typeof(T) == typeof(String))
			fm2.Verify(x => x(It.IsAny<String>()), Times.Once);
		else
			fm3.Verify(x => x(It.IsAny<Boolean>()), Times.Once);
	}

	[Fact]
	public void Arity3_When_Reindex_InlineUnmanagedArm_RoundTrips()
	{
		var g = Guid.NewGuid();
		Du<Boolean, Guid, Int32> du = g;

		var rest = du.When((Boolean _) => { });

		rest.TryPick<Guid>(out var got).Should().BeTrue();
		got.Should().Be(g);
	}

	[Fact]
	public void Arity3_When_Reindex_BoxedValueTypeArm_RoundTrips()
	{
		var s = new HasRef("payload");
		Du<Boolean, HasRef, Int32> du = s;

		var rest = du.When((Boolean _) => { });

		rest.TryPick<HasRef>(out var got).Should().BeTrue();
		got!.Text.Should().Be("payload");
	}

	[Fact]
	public void Arity3_When_Reindex_ReferenceTypeArm_RoundTrips()
	{
		Du<Boolean, String, Int32> du = "ref-payload";

		var rest = du.When((Boolean _) => { });

		rest.TryPick<String>(out var got).Should().BeTrue();
		got.Should().Be("ref-payload");
	}

	[Fact]
	public void Arity3_When_PreservesPositionWhenMatchedArmIsAfter()
	{
		Du<Int32, String, Boolean> du = 99;

		Du<Int32, String> rest = du.When((Boolean _) => { });

		rest.TryPick<Int32>(out var got).Should().BeTrue();
		got.Should().Be(99);
	}

	[Fact]
	public void Arity3_When_WhenMatched_RestIsConsumed_ChainContinuationIsNoOp()
	{
		Du<Int32, String, Boolean> du = 1;
		Du<String, Boolean> rest = du.When((Int32 _) => { });

		var ran = false;
		var act = () => rest.When((String _) => { ran = true; });
		act.Should().NotThrow();
		ran.Should().BeFalse();
	}

	[Fact]
	public void Arity2_PeeledDu_RoundTripsExhaustiveMatch_NoneSide()
	{
		Du<Int32, String> du = 9;
		var rest = du.When((Int32 _) => { });

		var label = rest.Match(s => $"str:{s}", _ => "none");
		label.Should().Be("none");
	}

	[Fact]
	public void Arity2_PeeledDu_RoundTripsExhaustiveMatch_PreservedSide()
	{
		Du<Int32, String> du = "alive";
		var rest = du.When((Int32 _) => { });

		var label = rest.Match(s => $"str:{s}", _ => "none");
		label.Should().Be("str:alive");
	}

	private record HasRef(String Text);
}
