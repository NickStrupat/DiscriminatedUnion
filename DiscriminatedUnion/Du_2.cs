using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using DiscriminatedUnion.Visitors;

#nullable enable

namespace DiscriminatedUnion;

// Hand-written specialization of Du<T1, T2> that stores each arm in its own
// strongly-typed field. This eliminates the Box<T> allocation that the shared
// (UnmanagedStorage, Object?) layout in Du.Init falls back to for value types
// that don't fit inline. The trade-off is a larger struct (sizeof(T1) +
// sizeof(T2) + 1 byte index + padding) — accepted for the most common arity.
[JsonConverter(typeof(DuJsonConverter))]
[DebuggerTypeProxy(typeof(Du.DebugView))]
public readonly struct Du<T1, T2>
	: IEquatable<Du<T1, T2>>
	, IDu<Du<T1, T2>>
	, IDuIndex
	where T1 : notnull
	where T2 : notnull
{
	private readonly T1 t1;
	private readonly T2 t2;
	private readonly Byte index;

	// Dispatch by _index, not by typeof(T): for Du<T, T> both arms have the
	// same T so the type check can't distinguish which slot was written.
	internal T Get<T>() where T : notnull
	{
		switch (index)
		{
			case 1: return Unsafe.As<T1, T>(ref Unsafe.AsRef(in t1));
			case 2: return Unsafe.As<T2, T>(ref Unsafe.AsRef(in t2));
			default: throw new InvalidInstanceException();
		}
	}

	internal Byte GetIndexUnsafe() => index;
	Byte IDuIndex.GetIndexUnsafe() => index;
	private Byte GetIndex() => index > 0 ? index : throw new InvalidInstanceException();

	public Du(T1 instance1) { t1 = instance1; t2 = default!; index = 1; }
	public Du(T2 instance2) { t1 = default!; t2 = instance2; index = 2; }

	public static implicit operator Du<T1, T2>(T1 value) => new(value);
	public static implicit operator Du<T1, T2>(T2 value) => new(value);

	public TResult Accept<TVisitor, TResult>(TVisitor visitor) where TVisitor : IVisitor<TResult>
		=> Accept<TVisitor, TResult>(ref visitor);

	public TResult Accept<TVisitor, TResult>(ref TVisitor visitor) where TVisitor : IVisitor<TResult>
	{
		switch (index)
		{
			case 1: return visitor.Visit(t1);
			case 2: return visitor.Visit(t2);
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
			case 2: visitor.Visit(t2); return;
			default: throw new InvalidInstanceException();
		}
	}

	public static void AcceptTypes<TTypeVisitor>(ref TTypeVisitor visitor) where TTypeVisitor : ITypeVisitor
	{
		visitor.Initialize<Du<T1, T2>>();
		if (visitor.VisitType<T1>()) return;
		if (visitor.VisitType<T2>()) return;
	}

	public static void AcceptTypes<TTypeVisitor, TRefParam>(ref TTypeVisitor visitor, ref TRefParam refParam) where TTypeVisitor : ITypeVisitor<TRefParam> where TRefParam : allows ref struct
	{
		visitor.Initialize<Du<T1, T2>>();
		if (visitor.VisitType<T1>(ref refParam)) return;
		if (visitor.VisitType<T2>(ref refParam)) return;
	}

	public static Boolean TryCreate<T>(T value, out Du<T1, T2> du)
	{
		if (value is null) { du = default; return false; }
		// Pass 1: exact static-type match. Required for value-type arms, and ensures an exact arm wins over an assignable one (e.g. string over object).
		if (typeof(T) == typeof(T1)) { du = new(Unsafe.As<T, T1>(ref value)); return true; }
		if (typeof(T) == typeof(T2)) { du = new(Unsafe.As<T, T2>(ref value)); return true; }
		// Pass 2: runtime-assignability fallback for derived/interface types. Leftmost arm wins.
		if (value is T1 v1) { du = new(v1); return true; }
		if (value is T2 v2) { du = new(v2); return true; }
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

	public TResult Match<TResult>(Func<T1, TResult> f1, Func<T2, TResult> f2)
	{
		switch (index)
		{
			case 1: return f1(t1);
			case 2: return f2(t2);
			default: throw new InvalidInstanceException();
		}
	}

	public void Switch(Action<T1> a1, Action<T2> a2)
	{
		switch (index)
		{
			case 1: a1(t1); break;
			case 2: a2(t2); break;
			default: throw new InvalidInstanceException();
		}
	}

	public static ImmutableArray<Type> Types { get; } = [
		typeof(T1),
		typeof(T2),
	];

	public override String ToString() => Accept<ToStringVisitor, String>(default);

	public override Int32 GetHashCode() => Accept<GetHashCodeVisitor, Int32>(default);

	public override Boolean Equals([NotNullWhen(true)] Object? obj) => obj switch
	{
		Du<T1, T2> du => Equals(du),
		T1 t when GetIndex() == 1 => t1.Equals(t),
		T2 t when GetIndex() == 2 => t2.Equals(t),
		IDu du => Accept<DuEqualityVisitor<IDu>, Boolean>(new(du)),
		_ => false
	};

	public Boolean Equals(Du<T1, T2> other) => Accept<DuEqualityVisitor<Du<T1, T2>>, Boolean>(new(other));

	public static Boolean operator ==(Du<T1, T2> left, Du<T1, T2> right) => left.Equals(right);
	public static Boolean operator !=(Du<T1, T2> left, Du<T1, T2> right) => !(left == right);
}
