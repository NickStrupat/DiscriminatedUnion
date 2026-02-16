using System.Runtime.CompilerServices;

namespace NickStrupat;

internal readonly struct ActionVisitor<T>(Action<T> action) : IVisitor<Null>
{
	Null IVisitor<Null>.Visit<TValue>(TValue value)
	{
		if (typeof(T) == typeof(TValue))
			action(Unsafe.As<TValue, T>(ref value));
		return default;
	}
}