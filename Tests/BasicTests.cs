using System.Runtime.CompilerServices;
using AwesomeAssertions;
using Castle.Core.Internal;
using Moq;
using NickStrupat;
using ObjectLayoutInspector;

[assembly: InternalsVisibleTo(InternalsVisible.ToDynamicProxyGenAssembly2)]

namespace Tests;

public class BasicTests
{
	[Fact]
	public void TestSize24() => TypeLayout.GetLayout<Du<Int32, String>>().Size.Should().Be(24);

	[Fact]
	public void DefaultInitialization_WhenSwitchIsInvoked_ThrowsInvalidInstanceException()
	{
		var am1 = new Mock<Action<Int32>>();
		var am2 = new Mock<Action<String>>();
		Du<Int32, String> du = default;

		var action = () => du.Switch(am1.Object, am2.Object);

		action.Should().Throw<InvalidInstanceException>();
		am1.Verify(x => x(It.IsAny<Int32>()), Times.Never);
		am2.Verify(x => x(It.IsAny<String>()), Times.Never);
	}

	[Fact]
	public void DefaultInitialization_WhenMatchIsInvoked_ThrowsInvalidInstanceException()
	{
		var fm1 = new Mock<Func<Int32, Int32>>();
		var fm2 = new Mock<Func<String, Int32>>();
		Du<Int32, String> du = default;

		var action = () => du.Match(fm1.Object, fm2.Object);

		action.Should().Throw<InvalidInstanceException>();
		fm1.Verify(x => x(It.IsAny<Int32>()), Times.Never);
		fm2.Verify(x => x(It.IsAny<String>()), Times.Never);
	}

	[Fact]
	public void ConstructionWithNull_Throws()
	{
		Du<Int32, String> du;
		var action = () => du = new(null!);
		action.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void TestSwitch_WithIntAndString()
	{
		var am1 = new Mock<Action<Int32>>();
		var am2 = new Mock<Action<String>>();
		Du<Int32, String> du = 42;

		du.Switch(am1.Object, am2.Object);

		am1.Verify(x => x(It.IsAny<Int32>()), Times.Once);
		am1.Verify(x => x(42), Times.Once);
		am2.Verify(x => x(It.IsAny<String>()), Times.Never);
	}

	[Fact]
	public void TestSwitch_WithInt()
	{
		var am1 = new Mock<Action<Int32>>();
		var am2 = new Mock<Action<String>>();
		Du<Int32, String> du = 1;

		du.Switch(am1.Object, am2.Object);

		am1.Verify(x => x(It.IsAny<Int32>()), Times.Once);
		am1.Verify(x => x(1), Times.Once);
		am2.Verify(x => x(It.IsAny<String>()), Times.Never);
	}

	[Fact]
	public void TestSwitch_WithString()
	{
		var am1 = new Mock<Action<Int32>>();
		var am2 = new Mock<Action<String>>();
		Du<Int32, String> du = new("1");

		du.Switch(am1.Object, am2.Object);

		am1.Verify(x => x(It.IsAny<Int32>()), Times.Never);
		am2.Verify(x => x(It.IsAny<String>()), Times.Once);
		am2.Verify(x => x("1"), Times.Once);
	}

	[Fact]
	public void TestMatch_WithInt()
	{
		var fm1 = new Mock<Func<Int32, String>>();
		var fm2 = new Mock<Func<String, String>>();
		Du<Int32, String> du = 1;

		var func = () => du.Match(fm1.Object, fm2.Object);

		func.Should().NotThrow();
		fm1.Verify(x => x(It.IsAny<Int32>()), Times.Once);
		fm1.Verify(x => x(1), Times.Once);
		fm2.Verify(x => x(It.IsAny<String>()), Times.Never);
	}

	[Fact]
	public void TestMatch_WithString()
	{
		var fm1 = new Mock<Func<Int32, String>>();
		var fm2 = new Mock<Func<String, String>>();
		Du<Int32, String> du = "test";

		var func = () => du.Match(fm1.Object, fm2.Object);

		func.Should().NotThrow();
		fm1.Verify(x => x(It.IsAny<Int32>()), Times.Never);
		fm2.Verify(x => x(It.IsAny<String>()), Times.Once);
		fm2.Verify(x => x("test"), Times.Once);
	}
}