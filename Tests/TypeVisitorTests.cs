using AwesomeAssertions;
using DiscriminatedUnion;
using DiscriminatedUnion.Visitors;

namespace Tests;

public class TypeVisitorTests
{
	[Fact]
	public void AcceptTypes()
	{
		TypeVisitor visitor = new();
		Du<Int32, String>.AcceptTypes(ref visitor);
		visitor.Types.Should().Equal(typeof(Int32), typeof(String));
	}
	[Fact]
	public void AcceptTypesRef()
	{
		TypeVisitorRef visitor = new();
		None none = new();
		Du<Int32, String>.AcceptTypes(ref visitor, ref none);
		visitor.Types.Should().Equal(typeof(Int32), typeof(String));
	}

	struct TypeVisitor : ITypeVisitor
	{
		private Int32 index;
		public Type[] Types { get; private set; }
		void ITypeVisitor.Initialize<TDu>() => Types = new Type[TDu.Types.Length];
		Boolean ITypeVisitor.VisitType<T>()
		{
			Types[index++] = typeof(T);
			return false;
		}
	}

	struct TypeVisitorRef : ITypeVisitor<None>
	{
		private Int32 index;
		public Type[] Types { get; private set; }
		void ITypeVisitor<None>.Initialize<TDu>() => Types = new Type[TDu.Types.Length];
		Boolean ITypeVisitor<None>.VisitType<T>(ref None refParam)
		{
			Types[index++] = typeof(T);
			return false;
		}
	}
}