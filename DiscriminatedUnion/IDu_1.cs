using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using DiscriminatedUnion.Visitors;

namespace DiscriminatedUnion;

public interface IDu<TDu> : IDu where TDu : IDu<TDu>
{
	static abstract Boolean TryCreate<T>(T value, [NotNullWhen(true)] out TDu? du);
	static virtual TDu Create<T>(T value) => TDu.TryCreate(value, out var du) ? du : throw new InvalidOperationException($"The value of type {typeof(T).FullName} cannot be converted to the discriminated union.");

	static abstract void AcceptTypes<TTypeVisitor>(ref TTypeVisitor visitor)
	where TTypeVisitor : ITypeVisitor;

	static abstract void AcceptTypes<TTypeVisitor, TRefParam>(ref TTypeVisitor visitor, ref TRefParam refParam)
		where TTypeVisitor : ITypeVisitor<TRefParam>
		where TRefParam : allows ref struct;

	static abstract ImmutableArray<Type> Types { get; }
}