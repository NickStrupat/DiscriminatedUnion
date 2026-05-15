using System.Diagnostics.CodeAnalysis;

namespace NickStrupat;

public interface IDu
{
	TResult Accept<TVisitor, TResult>(TVisitor visitor) where TVisitor : IVisitor<TResult> => Accept<TVisitor, TResult>(ref visitor);
	TResult Accept<TVisitor, TResult>(ref TVisitor visitor) where TVisitor : IVisitor<TResult>;

	Boolean TryPick<T>([NotNullWhen(true)] out T? matched) where T : notnull;
}