using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NickStrupat;

internal struct TryPickVisitor<T> : IVisitor<Boolean> where T : notnull
{
	public T? Picked { get; private set; }

	[MemberNotNullWhen(true, nameof(Picked))]
	Boolean IVisitor<Boolean>.Visit<TValue>(TValue value)
	{
		if (typeof(T) != typeof(TValue))
			return false;
		Picked = Unsafe.As<TValue, T>(ref value);
		return true;
	}
}