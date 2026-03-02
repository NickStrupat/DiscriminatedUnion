using System.Diagnostics.CodeAnalysis;

namespace NickStrupat;

public interface IDu<TDu> : IDu where TDu : IDu<TDu>
{
	static abstract Boolean TryCreate<T>(T value, [NotNullWhen(true)] out TDu? du);
	static virtual TDu Create<T>(T value) => TDu.TryCreate(value, out var du) ? du : throw new InvalidOperationException($"The value of type {typeof(T).FullName} cannot be converted to the discriminated union.");
}