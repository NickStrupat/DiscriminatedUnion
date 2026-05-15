using System.Runtime.CompilerServices;

namespace DiscriminatedUnion.Visitors;

internal readonly struct ActionVisitor<T>(Action<T> action) : IVisitor
{
	void IVisitor.Visit<TValue>(TValue value)
	{
		if (typeof(T) == typeof(TValue))
			action(Unsafe.As<TValue, T>(ref value));
	}
}