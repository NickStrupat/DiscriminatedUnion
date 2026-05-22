using AwesomeAssertions;
using DiscriminatedUnion;
using DiscriminatedUnion.Extensions;

namespace Tests;

public class PickTests
{
	// Note: parameter type for arm T is `T?`. For value-type arms that's just T (nullability annotation
	// is a no-op on value types under `notnull`), so callers use `out Int32 v`, not `out Int32? v`.

	[Fact]
	public void Pick_TwoArm_MatchingValueArm_ReturnsHandledAndSetsMatched()
	{
		Du<Int32, String> du = 42;

		Du<String, None> residual = du.Pick(out Int32 matched);

		residual.TryPick<None>(out _).Should().BeTrue();
		matched.Should().Be(42);
	}

	[Fact]
	public void Pick_TwoArm_NonMatchingArm_ReturnsResidualWithNonePadding()
	{
		Du<Int32, String> du = "hello";

		Du<String, None> residual = du.Pick(out Int32 matched);

		matched.Should().Be(0);
		residual.TryPick<String>(out var s).Should().BeTrue();
		s.Should().Be("hello");
	}

	[Fact]
	public void Pick_TwoArm_PickReferenceArm_NonMatch_MatchedIsNull()
	{
		Du<Int32, String> du = 7;

		Du<Int32, None> residual = du.Pick(out String? matched);

		matched.Should().BeNull();
		residual.TryPick<Int32>(out var i).Should().BeTrue();
		i.Should().Be(7);
	}

	[Fact]
	public void Pick_ThreeArm_PickMiddle_ResidualPreservesOuterArmsInOrder()
	{
		Du<Int32, String, Double> du = 3.14;

		Du<Du<Int32, Double>, None> residual = du.Pick(out String? matched);

		matched.Should().BeNull();
		residual.TryPick<Du<Int32, Double>>(out var inner).Should().BeTrue();
		inner.TryPick<Double>(out var d).Should().BeTrue();
		d.Should().Be(3.14);
	}

	[Fact]
	public void Pick_ThreeArm_PickFirst_ResidualIsTailArms()
	{
		Du<Int32, String, Double> du = "hi";

		Du<Du<String, Double>, None> residual = du.Pick(out Int32 matched);

		matched.Should().Be(0);
		residual.TryPick<Du<String, Double>>(out var inner).Should().BeTrue();
		inner.TryPick<String>(out var s).Should().BeTrue();
		s.Should().Be("hi");
	}

	[Fact]
	public void Pick_ThreeArm_PickLast_ResidualIsHeadArms()
	{
		Du<Int32, String, Double> du = 5;

		Du<Du<Int32, String>, None> residual = du.Pick(out Double matched);

		matched.Should().Be(0.0);
		residual.TryPick<Du<Int32, String>>(out var inner).Should().BeTrue();
		inner.TryPick<Int32>(out var i).Should().BeTrue();
		i.Should().Be(5);
	}

	[Fact]
	public void Pick_ThreeArm_MatchingArm_ReturnsHandled()
	{
		Du<Int32, String, Double> du = 3.14;

		Du<Du<Int32, String>, None> residual = du.Pick(out Double matched);

		residual.TryPick<None>(out _).Should().BeTrue();
		matched.Should().Be(3.14);
	}

	[Fact]
	public void Pick_FourArm_PickMiddleArm_ResidualHoldsOtherArmValue()
	{
		Du<Int32, String, Double, Boolean> du = 9;

		Du<Du<Int32, Double, Boolean>, None> residual = du.Pick(out String? matched);

		matched.Should().BeNull();
		residual.TryPick<Du<Int32, Double, Boolean>>(out var inner).Should().BeTrue();
		inner.TryPick<Int32>(out var i).Should().BeTrue();
		i.Should().Be(9);
	}

	[Fact]
	public void Pick_ComposesAcrossArities_PeelDownToSingleValue()
	{
		Du<Int32, String, Double> du = 3.14;

		var afterIntPeel = du.Pick(out Int32 _);   // Du<Du<String, Double>, None>
		afterIntPeel.TryPick<None>(out _).Should().BeFalse();

		var afterStringPeel = afterIntPeel.Pick(out String? _); // Du<Double, None>
		afterStringPeel.TryPick<None>(out _).Should().BeFalse();

		None afterDoublePeel = afterStringPeel.Pick(out Double d);
		d.Should().Be(3.14);
	}

	[Fact]
	public void Pick_ComposesAcrossArities_StopsAtFirstMatch()
	{
		Du<Int32, String, Double> du = "hit";

		var afterIntPeel = du.Pick(out Int32 _);
		afterIntPeel.TryPick<None>(out _).Should().BeFalse();

		var afterStringPeel = afterIntPeel.Pick(out String? matched);
		afterStringPeel.TryPick<None>(out _).Should().BeTrue();
		matched.Should().Be("hit");
	}

	[Fact]
	public void Pick_OnDefaultDu_Throws()
	{
		Du<Int32, String> du = default;

		Action act = () => du.Pick(out Int32 _);

		act.Should().Throw<InvalidInstanceException>();
	}

	[Fact]
	public void Pick_NonMatchingArm_LeavesResidualValueIntactForRoundTrip()
	{
		Du<Int32, String, Double> du = "preserve me";

		Du<Du<Int32, String>, None> residual = du.Pick(out Double _);

		residual.TryPick<Du<Int32, String>>(out var inner).Should().BeTrue();
		inner.Should().Be(new Du<Int32, String>("preserve me"));
	}

	[Fact]
	public void Pick_DuWithNonePadding_PickingValueArm_TerminatesWithNoneResidual()
	{
		Du<Int32, None> du = 42;

		None residual = du.Pick(out Int32 matched);
		matched.Should().Be(42);
	}

	[Fact]
	public void Pick_DuWithNonePadding_HoldingNone_ReturnsTerminator()
	{
		Du<String, None> du = default(None);

		None residual = du.Pick(out String? matched);

		matched.Should().BeNull();
		// None terminator can't structurally distinguish handled/unhandled; assert on matched only.
	}

	[Fact]
	public void Pick_FullChain_EndsAtNone()
	{
		Du<Int32, String, Double> du = 3.14;

		var afterInt = du.Pick(out Int32 _);            // Du<Du<String, Double>, None>
		var afterString = afterInt.Pick(out String? _); // Du<Double, None>
		None terminator = afterString.Pick(out Double d);
		d.Should().Be(3.14);
	}

	[Fact]
	public void Pick_Composes_MatchOnLastPeel()
	{
		Du<Int32, String, Double> du = 3.14;

		Int32 i = -1;
		String? s = "sentinel";
		Double d = -1;

		None terminator = du
			.Pick(out i)
			.Pick(out s)
			.Pick(out d);
		i.Should().Be(0);       // miss → overwritten to default
		s.Should().BeNull();    // miss → overwritten to default
		d.Should().Be(3.14);    // matched
	}

	[Fact]
	public void Pick_Composes_MatchOnFirstPeel_LaterStepsDefaultTheirOutParams()
	{
		// New semantics: chain always runs all steps. Once handled, subsequent
		// Picks see "handled" state and write `default` to their out params
		// (C# definite-assignment forces an out assignment).
		Du<Int32, String, Double> du = 42;

		Int32 i = -1;
		String? s = "sentinel";
		Double d = -1;

		None terminator = du
			.Pick(out i)
			.Pick(out s)
			.Pick(out d);
		i.Should().Be(42);   // matched
		s.Should().BeNull(); // miss after handled → default
		d.Should().Be(0.0);  // miss after handled → default
	}

	[Fact]
	public void Pick_Composes_MatchOnMiddlePeel_LaterStepsDefaultTheirOutParams()
	{
		Du<Int32, String, Double> du = "hit";

		Int32 i = -1;
		String? s = "sentinel";
		Double d = -1;

		None terminator = du
			.Pick(out i)
			.Pick(out s)
			.Pick(out d);
		i.Should().Be(0);    // miss → default
		s.Should().Be("hit"); // matched
		d.Should().Be(0.0);  // miss after handled → default
	}
}
