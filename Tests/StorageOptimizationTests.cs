using System.Runtime.CompilerServices;
using DiscriminatedUnion;

namespace Tests;

public class StorageOptimizationTests
{
	[Fact]
	public void DUSize()
	{
		Assert.Equal(16, Unsafe.SizeOf<Du<Int32, Object>>());
	}

	// [Fact]
	// public void TestMap()
	// {
	// 	for (var i = 0; i != 256; i++)
	// 	{
	// 		var obj = Du.GetIndexObject((Byte)i);
	// 		Du.TryGetIndex(obj, out var index);
	// 		Assert.Equal(i, index);
	// 	}
	// }
}