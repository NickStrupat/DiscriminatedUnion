using System.Runtime.CompilerServices;

namespace NickStrupat;

internal readonly struct DuEqualityVisitor(IDu other) : IVisitor<Boolean>
{
	Boolean IVisitor<Boolean>.Visit<T>(T value) => other.Visit<DuEqualityVisitorInternal<T>, Boolean>(new(value));

	private readonly struct DuEqualityVisitorInternal<T>(T value) : IVisitor<Boolean>
	{
		Boolean IVisitor<Boolean>.Visit<TOther>(TOther other)
		{
			return typeof(T) == typeof(TOther) && EqualityComparer<T>.Default.Equals(value, Unsafe.As<TOther, T>(ref other));
		}
	}
}