using AwesomeAssertions;
using DiscriminatedUnion;
using DiscriminatedUnion.Extensions;

namespace Tests;

public class PickTests
{
	// Note: parameter type for arm T is `T?`. For value-type arms that's just T (nullability annotation
	// is a no-op on value types under `notnull`), so callers use `out Int32 v`, not `out Int32? v`.

	[Fact]
	public void Pick_TwoArm_MatchingValueArm_ReturnsNullAndSetsMatched()
	{
		Du<Int32, String> du = 42;

		var residual = du.Pick(out Int32 matched);

		residual.Should().BeNull();
		matched.Should().Be(42);
	}

	[Fact]
	public void Pick_TwoArm_NonMatchingArm_ReturnsResidualWithNonePadding()
	{
		Du<Int32, String> du = "hello";

		Du<String, None>? residual = du.Pick(out Int32 matched);

		matched.Should().Be(0);
		residual.Should().NotBeNull();
		residual!.Value.TryPick<String>(out var s).Should().BeTrue();
		s.Should().Be("hello");
	}

	[Fact]
	public void Pick_TwoArm_PickReferenceArm_NonMatch_MatchedIsNull()
	{
		Du<Int32, String> du = 7;

		Du<Int32, None>? residual = du.Pick(out String? matched);

		matched.Should().BeNull();
		residual.Should().NotBeNull();
		residual!.Value.TryPick<Int32>(out var i).Should().BeTrue();
		i.Should().Be(7);
	}

	[Fact]
	public void Pick_ThreeArm_PickMiddle_ResidualPreservesOuterArmsInOrder()
	{
		Du<Int32, String, Double> du = 3.14;

		Du<Int32, Double>? residual = du.Pick(out String? matched);

		matched.Should().BeNull();
		residual.Should().NotBeNull();
		residual!.Value.TryPick<Double>(out var d).Should().BeTrue();
		d.Should().Be(3.14);
	}

	[Fact]
	public void Pick_ThreeArm_PickFirst_ResidualIsTailArms()
	{
		Du<Int32, String, Double> du = "hi";

		Du<String, Double>? residual = du.Pick(out Int32 matched);

		matched.Should().Be(0);
		residual.Should().NotBeNull();
		residual!.Value.TryPick<String>(out var s).Should().BeTrue();
		s.Should().Be("hi");
	}

	[Fact]
	public void Pick_ThreeArm_PickLast_ResidualIsHeadArms()
	{
		Du<Int32, String, Double> du = 5;

		Du<Int32, String>? residual = du.Pick(out Double matched);

		matched.Should().Be(0.0);
		residual.Should().NotBeNull();
		residual!.Value.TryPick<Int32>(out var i).Should().BeTrue();
		i.Should().Be(5);
	}

	[Fact]
	public void Pick_ThreeArm_MatchingArm_ReturnsNull()
	{
		Du<Int32, String, Double> du = 3.14;

		var residual = du.Pick(out Double matched);

		residual.Should().BeNull();
		matched.Should().Be(3.14);
	}

	[Fact]
	public void Pick_FourArm_PickMiddleArm_ResidualHoldsOtherArmValue()
	{
		Du<Int32, String, Double, Boolean> du = 9;

		Du<Int32, Double, Boolean>? residual = du.Pick(out String? matched);

		matched.Should().BeNull();
		residual.Should().NotBeNull();
		residual!.Value.TryPick<Int32>(out var i).Should().BeTrue();
		i.Should().Be(9);
	}

	[Fact]
	public void Pick_ComposesAcrossArities_PeelDownToSingleValue()
	{
		Du<Int32, String, Double> du = 3.14;

		var afterIntPeel = du.Pick(out Int32 _);
		afterIntPeel.Should().NotBeNull();

		var afterStringPeel = afterIntPeel!.Value.Pick(out String? _);
		afterStringPeel.Should().NotBeNull();

		var afterDoublePeel = afterStringPeel!.Value.Pick(out Double d);
		afterDoublePeel.Should().BeNull();
		d.Should().Be(3.14);
	}

	[Fact]
	public void Pick_ComposesAcrossArities_StopsAtFirstMatch()
	{
		Du<Int32, String, Double> du = "hit";

		var afterIntPeel = du.Pick(out Int32 _);
		afterIntPeel.Should().NotBeNull();

		var afterStringPeel = afterIntPeel!.Value.Pick(out String? matched);
		afterStringPeel.Should().BeNull();
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

		var residual = du.Pick(out Double _);

		residual.Should().NotBeNull();
		residual!.Value.Should().Be(new Du<Int32, String>("preserve me"));
	}

	[Fact]
	public void Pick_DuWithNonePadding_PickingValueArm_TerminatesWithNoneResidual()
	{
		// Specialized Du<T, None>.Pick(out T?) returns None? (not Du<None, None>?), forming the
		// natural terminator for a chained-Pick peel.
		Du<Int32, None> du = 42;

		None? residual = du.Pick(out Int32 matched);

		residual.Should().BeNull();
		matched.Should().Be(42);
	}

	[Fact]
	public void Pick_DuWithNonePadding_HoldingNone_ReturnsNoneSingleton()
	{
		Du<String, None> du = default(None);

		None? residual = du.Pick(out String? matched);

		matched.Should().BeNull();
		residual.Should().NotBeNull();
		residual!.Value.Should().Be(default(None));
	}

	[Fact]
	public void Pick_FullChain_EndsAtNone()
	{
		Du<Int32, String, Double> du = 3.14;

		var afterInt = du.Pick(out Int32 _);            // Du<String, Double>?
		var afterString = afterInt!.Value.Pick(out String? _); // Du<Double, None>?
		None? terminator = afterString!.Value.Pick(out Double d); // terminator is None?

		terminator.Should().BeNull();
		d.Should().Be(3.14);
	}

	[Fact]
	public void Pick_Composes_MatchOnLastPeel()
	{
		// Fluent chain via ?. lifting through Nullable<Du<...>>. Last peel matches → terminator is null.
		Du<Int32, String, Double> du = 3.14;

		Int32 i = -1;
		String? s = "sentinel";
		Double d = -1;

		None? terminator = du
			.Pick(out i)
			?.Pick(out s)
			?.Pick(out d);

		terminator.Should().BeNull();
		i.Should().Be(0);       // miss → overwritten to default
		s.Should().BeNull();    // miss → overwritten to default
		d.Should().Be(3.14);    // matched
	}

	[Fact]
	public void Pick_Composes_MatchOnFirstPeel_ShortCircuitsLaterOutParams()
	{
		// First peel matches → returns null → subsequent ?.Pick calls are skipped entirely,
		// so their out parameters retain whatever value they held before the chain.
		Du<Int32, String, Double> du = 42;

		Int32 i = -1;
		String? s = "sentinel";
		Double d = -1;

		None? terminator = du
			.Pick(out i)
			?.Pick(out s)
			?.Pick(out d);

		terminator.Should().BeNull();
		i.Should().Be(42);      // matched
		s.Should().Be("sentinel");  // untouched (short-circuited)
		d.Should().Be(-1);          // untouched (short-circuited)
	}

	[Fact]
	public void Pick_Composes_MatchOnMiddlePeel_ShortCircuitsAfterMatch()
	{
		Du<Int32, String, Double> du = "hit";

		Int32 i = -1;
		String? s = "sentinel";
		Double d = -1;

		None? terminator = du
			.Pick(out i)
			?.Pick(out s)
			?.Pick(out d);

		terminator.Should().BeNull();
		i.Should().Be(0);           // miss → default
		s.Should().Be("hit");       // matched
		d.Should().Be(-1);          // untouched (short-circuited)
	}
}
