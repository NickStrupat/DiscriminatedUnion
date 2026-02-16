using System.Collections.Immutable;

namespace NickStrupat;

public interface IDu
{
	TResult Visit<TVisitor, TResult>(ref TVisitor visitor) where TVisitor : IVisitor<TResult>;
	static virtual ImmutableArray<Type> Types => throw new NotImplementedException();
}