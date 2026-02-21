using System.Runtime.CompilerServices;

namespace NickStrupat;

internal readonly struct DuEqualityVisitor<TDu>(TDu other) : IVisitor<Boolean> where TDu : IDu
{
	Boolean IVisitor<Boolean>.Visit<T>(T value)
	{
		DuEqualityVisitorInternal<T> visitor = new(value);
		return other.Accept<DuEqualityVisitorInternal<T>, Boolean>(ref visitor);
	}

	private readonly struct DuEqualityVisitorInternal<T>(T value) : IVisitor<Boolean> where T : notnull
	{
		Boolean IVisitor<Boolean>.Visit<TOther>(TOther other)
		{
			if (!typeof(TOther).IsValueType && value.Equals(other))
				return true;
			return typeof(T) == typeof(TOther) && EqualityComparer<T>.Default.Equals(value, Unsafe.As<TOther, T>(ref other));
		}
	}
}