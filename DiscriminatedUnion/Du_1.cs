using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using DiscriminatedUnion.Visitors;

#nullable enable

namespace DiscriminatedUnion;

// 1-arity Du<T>. Degenerate as a "union" (one arm, no choice to discriminate)
// but used as the structural sentinel that lets residual-chain extensions
// (Pick/When/|/Else and their Func duals) terminate unambiguously: the
// terminator extension is on Du<Du<T1>, None> / Du<Du<T1>, R> shapes, where the
// 1-armed inner is structurally distinct from any 2-armed Du<X, Y> general
// receiver.
[JsonConverter(typeof(DuJsonConverter))]
[DebuggerTypeProxy(typeof(Du.DebugView))]
public readonly struct Du<T1>
	: IEquatable<Du<T1>>
	, IDu<Du<T1>>
	, IDuIndex
	where T1 : notnull
{
	private readonly T1 t1;
	private readonly Byte index;

	internal T Get<T>() where T : notnull
	{
		switch (index)
		{
			case 1: return Unsafe.As<T1, T>(ref Unsafe.AsRef(in t1));
			default: throw new InvalidInstanceException();
		}
	}

	internal Byte GetIndexUnsafe() => index;
	Byte IDuIndex.GetIndexUnsafe() => index;
	private Byte GetIndex() => index > 0 ? index : throw new InvalidInstanceException();

	public Du(T1 instance1) { t1 = instance1; index = 1; }

	public static implicit operator Du<T1>(T1 value) => new(value);

	public TResult Accept<TVisitor, TResult>(TVisitor visitor) where TVisitor : IVisitor<TResult>
		=> Accept<TVisitor, TResult>(ref visitor);

	public TResult Accept<TVisitor, TResult>(ref TVisitor visitor) where TVisitor : IVisitor<TResult>
	{
		switch (index)
		{
			case 1: return visitor.Visit(t1);
			default: throw new InvalidInstanceException();
		}
	}

	public void Accept<TVisitor>(TVisitor visitor) where TVisitor : IVisitor
		=> Accept<TVisitor>(ref visitor);

	public void Accept<TVisitor>(ref TVisitor visitor) where TVisitor : IVisitor
	{
		switch (index)
		{
			case 1: visitor.Visit(t1); return;
			default: throw new InvalidInstanceException();
		}
	}

	public static void AcceptTypes<TTypeVisitor>(ref TTypeVisitor visitor) where TTypeVisitor : ITypeVisitor
	{
		visitor.Initialize<Du<T1>>();
		if (visitor.VisitType<T1>()) return;
	}

	public static void AcceptTypes<TTypeVisitor, TRefParam>(ref TTypeVisitor visitor, ref TRefParam refParam) where TTypeVisitor : ITypeVisitor<TRefParam> where TRefParam : allows ref struct
	{
		visitor.Initialize<Du<T1>>();
		if (visitor.VisitType<T1>(ref refParam)) return;
	}

	public static Boolean TryCreate<T>(T value, out Du<T1> du)
	{
		if (value is null) { du = default; return false; }
		if (typeof(T) == typeof(T1)) { du = new(Unsafe.As<T, T1>(ref value)); return true; }
		if (value is T1 v1) { du = new(v1); return true; }
		du = default;
		return false;
	}

	public Boolean TryPick<T>([NotNullWhen(true)] out T? matched) where T : notnull
	{
		var visitor = new TryPickVisitor<T>();
		var result = this.Accept<TryPickVisitor<T>, Boolean>(ref visitor);
		matched = visitor.Picked!;
		return result;
	}

	public TResult Match<TResult>(Func<T1, TResult> f1)
	{
		switch (index)
		{
			case 1: return f1(t1);
			default: throw new InvalidInstanceException();
		}
	}

	public void Switch(Action<T1> a1)
	{
		switch (index)
		{
			case 1: a1(t1); break;
			default: throw new InvalidInstanceException();
		}
	}

	public static ImmutableArray<Type> Types { get; } = [
		typeof(T1),
	];

	public override String ToString() => Accept<ToStringVisitor, String>(default);

	public override Int32 GetHashCode() => Accept<GetHashCodeVisitor, Int32>(default);

	public override Boolean Equals([NotNullWhen(true)] Object? obj) => obj switch
	{
		Du<T1> du => Equals(du),
		T1 t when GetIndex() == 1 => t1.Equals(t),
		IDu du => Accept<DuEqualityVisitor<IDu>, Boolean>(new(du)),
		_ => false
	};

	public Boolean Equals(Du<T1> other) => Accept<DuEqualityVisitor<Du<T1>>, Boolean>(new(other));

	public static Boolean operator ==(Du<T1> left, Du<T1> right) => left.Equals(right);
	public static Boolean operator !=(Du<T1> left, Du<T1> right) => !(left == right);
}
