using AwesomeAssertions;
using DiscriminatedUnion;
using DiscriminatedUnion.Extensions;

namespace Tests;

public class PipeResultTests
{
	[Fact]
	public void Pipe_TwoArm_MatchingArm_ProducesResidualHoldingR()
	{
		Du<Int32, String> du = 42;

		Du<Du<String>, String> residual = du | ((Int32 i) => $"int:{i}");

		residual.TryPick<String>(out var r).Should().BeTrue();
		r.Should().Be("int:42");
	}

	[Fact]
	public void Pipe_ThreeArm_FluentChain_CollapsesToR()
	{
		Du<Int32, String, Double> du = "hit";

		String result = du
			| ((Int32 i) => $"int:{i}")
			| ((String s) => $"str:{s}")
			| ((Double d) => $"dbl:{d}");

		result.Should().Be("str:hit");
	}

	[Fact]
	public void Pipe_ThreeArm_FluentChain_LastArmMatches()
	{
		Du<Int32, String, Double> du = 3.14;

		String result = du
			| ((Int32 i) => $"int:{i}")
			| ((String s) => $"str:{s}")
			| ((Double d) => $"dbl:{d}");

		result.Should().Be("dbl:3.14");
	}

	[Fact]
	public void Pipe_ElseCatchAll_OnEntry_DispatchesToCorrectArm()
	{
		Du<Int32, String, Double> du = "x";

		String result = du | ((Else e) => $"any:{e.Value}");

		result.Should().Be("any:x");
	}

	[Fact]
	public void Pipe_ElseCatchAll_AfterPartialChain_HandlesRemainingArm()
	{
		Du<Int32, String, Double> du = 3.14;

		String result = du
			| ((Int32 i) => $"int:{i}")
			| ((Else e) => $"any:{e.Value}");

		result.Should().Be("any:3.14");
	}

	[Fact]
	public void Pipe_ElseCatchAll_AfterPartialChain_AlreadyHandled_PropagatesR()
	{
		Du<Int32, String, Double> du = 7;

		String result = du
			| ((Int32 i) => $"int:{i}")
			| ((Else e) => $"any:{e.Value}");

		result.Should().Be("int:7");
	}

	[Fact]
	public void Pipe_Parameterless_OnEntry_AlwaysInvokesHandler()
	{
		Du<Int32, String> du = "hello";

		String result = du | (() => "fallback");

		result.Should().Be("fallback");
	}

	[Fact]
	public void Pipe_Parameterless_AfterChain_AlreadyHandled_PropagatesR()
	{
		Du<Int32, String, Double> du = 7;

		String result = du
			| ((Int32 i) => $"int:{i}")
			| (() => "fallback");

		result.Should().Be("int:7");
	}

	[Fact]
	public void Pipe_OnDefaultDu_Throws()
	{
		Du<Int32, String> du = default;

		Action act = () => { _ = du | ((Int32 i) => i.ToString()); };

		act.Should().Throw<InvalidInstanceException>();
	}

	[Fact]
	public void Pipe_MixedWithWhen_Composes()
	{
		Du<Int32, String, Double> du = 3.14;

		String result = du
			.When((Int32 i) => $"int:{i}")
			| ((String s) => $"str:{s}")
			| ((Double d) => $"dbl:{d}");

		result.Should().Be("dbl:3.14");
	}
}
