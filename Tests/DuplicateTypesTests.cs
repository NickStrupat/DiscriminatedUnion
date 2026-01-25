using Moq;
using NickStrupat;

namespace Tests;

public class DuplicateTypesTests
{
	[Fact]
	public void TwoStrings()
	{
		var ma1 = new Mock<Action<String>>();
		var ma2 = new Mock<Action<String>>();

		Du<String, String> du = new(instance1: "test"); // Add the parameter name to disambiguate the constructors
		du.Switch(ma1.Object, ma2.Object);

		ma1.Verify(x => x(It.IsAny<String>()), Times.Once);
		ma1.Verify(x => x("test"), Times.Once);
		ma2.Verify(x => x(It.IsAny<String>()), Times.Never);
	}
}