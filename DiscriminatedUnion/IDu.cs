using System.Collections.Immutable;

namespace NickStrupat;

public interface IDu
{
	TResult Accept<TVisitor, TResult>(TVisitor visitor) where TVisitor : IVisitor<TResult> => Accept<TVisitor, TResult>(ref visitor);
	TResult Accept<TVisitor, TResult>(ref TVisitor visitor) where TVisitor : IVisitor<TResult>;
	static virtual ImmutableArray<Type> Types => throw new NotImplementedException();
}