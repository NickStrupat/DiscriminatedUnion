using System.Diagnostics.CodeAnalysis;
using DiscriminatedUnion.Visitors;

namespace DiscriminatedUnion;

public interface IDu
{
	TResult Accept<TVisitor, TResult>(TVisitor visitor) where TVisitor : IVisitor<TResult> => Accept<TVisitor, TResult>(ref visitor);
	TResult Accept<TVisitor, TResult>(ref TVisitor visitor) where TVisitor : IVisitor<TResult>;

	void Accept<TVisitor>(TVisitor visitor) where TVisitor : IVisitor => Accept<TVisitor>(ref visitor);
	void Accept<TVisitor>(ref TVisitor visitor) where TVisitor : IVisitor;

	Boolean TryPick<T>([NotNullWhen(true)] out T? matched) where T : notnull;
}