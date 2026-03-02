using System.Collections.Immutable;

namespace NickStrupat;

public interface IDu
{
	TResult Accept<TVisitor, TResult>(TVisitor visitor) where TVisitor : IVisitor<TResult> => Accept<TVisitor, TResult>(ref visitor);
	TResult Accept<TVisitor, TResult>(ref TVisitor visitor) where TVisitor : IVisitor<TResult>;

	static virtual void AcceptTypes<TTypeVisitor, TRefParam>(ref TTypeVisitor visitor, ref TRefParam refParam)
	where TTypeVisitor : ITypeVisitor<TRefParam>
	where TRefParam : allows ref struct => throw new NotImplementedException();

	static virtual ImmutableArray<Type> Types => throw new NotImplementedException();
}