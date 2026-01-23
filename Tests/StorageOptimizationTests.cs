using System.Runtime.CompilerServices;
using NickStrupat;

namespace Tests;

public class StorageOptimizationTests
{
	[Fact]
	public void DUSize()
	{
		Assert.Equal(24, Unsafe.SizeOf<DU<Int32, Object>>());
	}

	[Fact]
	public void TestMap()
	{
		for (var i = 0; i != 256; i++)
		{
			var obj = DU.GetIndexObject((Byte)i);
			DU.TryGetIndex(obj, out var index);
			Assert.Equal(i, index);
		}
	}
}