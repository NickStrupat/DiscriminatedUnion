using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace NickStrupat;

public interface IDu
{
	Boolean TryPick<T>([NotNullWhen(true)] out T? matched) where T : notnull;
	void Visit<T>(Action<T> action) where T : notnull;
	void Visit<TVisitor>(TVisitor visitor) where TVisitor : IVisitor;
	TResult Visit<TVisitor, TResult>(TVisitor visitor) where TVisitor : IVisitor<TResult>;

	static abstract ImmutableArray<Type> Types { get; }
}