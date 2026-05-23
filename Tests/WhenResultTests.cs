using AwesomeAssertions;
using DiscriminatedUnion;
using DiscriminatedUnion.Extensions;

namespace Tests;

public class WhenResultTests
{
	[Fact]
	public void When_TwoArm_MatchingArm_InvokesHandlerAndPlacesResultInResidual()
	{
		Du<Int32, String> du = 42;

		Du<Du<String>, String> residual = du.When((Int32 i) => $"int:{i}");

		residual.TryPick<String>(out var r).Should().BeTrue();
		r.Should().Be("int:42");
	}

	[Fact]
	public void When_TwoArm_NonMatchingArm_PropagatesNonMatchedValue()
	{
		Du<Int32, String> du = "hello";

		Du<Du<String>, String> residual = du.When((Int32 i) => $"int:{i}");

		residual.TryPick<Du<String>>(out var inner).Should().BeTrue();
		inner.TryPick<String>(out var s).Should().BeTrue();
		s.Should().Be("hello");
	}

	[Fact]
	public void When_ThreeArm_MiddleArmMatches_PlacesResultInResidual()
	{
		Du<Int32, String, Double> du = "hit";

		Du<Du<Int32, Double>, Int32> residual = du.When((String s) => s.Length);

		residual.TryPick<Int32>(out var r).Should().BeTrue();
		r.Should().Be(3);
	}

	[Fact]
	public void When_ThreeArm_NonMatch_ResidualCarriesOtherArm()
	{
		Du<Int32, String, Double> du = 3.14;

		Du<Du<Int32, Double>, String> residual = du.When((String s) => s);

		residual.TryPick<Du<Int32, Double>>(out var inner).Should().BeTrue();
		inner.TryPick<Double>(out var d).Should().BeTrue();
		d.Should().Be(3.14);
	}

	[Fact]
	public void When_FluentChain_CollapsesToHandlerResult()
	{
		Du<Int32, String, Double> du = "hit";

		String result = du
			.When((Int32 i) => $"int:{i}")
			.When((String s) => $"str:{s}")
			.When((Double d) => $"dbl:{d}");

		result.Should().Be("str:hit");
	}

	[Fact]
	public void When_FluentChain_LastArmMatches_LastHandlerProducesResult()
	{
		Du<Int32, String, Double> du = 3.14;

		String result = du
			.When((Int32 i) => $"int:{i}")
			.When((String s) => $"str:{s}")
			.When((Double d) => $"dbl:{d}");

		result.Should().Be("dbl:3.14");
	}

	[Fact]
	public void When_FluentChain_FirstArmMatches_PropagatesThroughR()
	{
		Du<Int32, String, Double> du = 7;

		String result = du
			.When((Int32 i) => $"int:{i}")
			.When((String s) => $"str:{s}")
			.When((Double d) => $"dbl:{d}");

		result.Should().Be("int:7");
	}

	[Fact]
	public void When_FourArm_FluentChain_CollapsesToR()
	{
		Du<Int32, String, Double, Boolean> du = true;

		String result = du
			.When((Int32 i) => $"int:{i}")
			.When((String s) => $"str:{s}")
			.When((Double d) => $"dbl:{d}")
			.When((Boolean b) => $"bool:{b}");

		result.Should().Be("bool:True");
	}

	[Fact]
	public void When_OnDefaultDu_Throws()
	{
		Du<Int32, String> du = default;

		Action act = () => du.When((Int32 i) => i.ToString());

		act.Should().Throw<InvalidInstanceException>();
	}

	[Fact]
	public void When_HandlerNotInvokedForNonMatchingArm()
	{
		var calls = 0;
		Du<Int32, String> du = "hello";

		_ = du.When((Int32 i) => { calls++; return i.ToString(); });

		calls.Should().Be(0);
	}
}
