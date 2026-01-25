using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NickStrupat;

[JsonConverter(typeof(DuConverter))]
public readonly struct Du<T1, T2>
	: IEquatable<Du<T1, T2>>
	, IDu<Du<T1, T2>>
	where T1 : notnull
	where T2 : notnull
{
	private readonly UnmanagedStorage unmanaged;
	private readonly Object? managed;

	private Byte GetIndex() => managed is { } mr && Du.TryGetIndex(mr, out var i) ? i : unmanaged._0;

	public Du(T1 instance1) => (managed, unmanaged) = Du.Init(ref instance1, 1);
	public Du(T2 instance2) => (managed, unmanaged) = Du.Init(ref instance2, 2);

	public static implicit operator Du<T1, T2>(T1 value) => new(value);
	public static implicit operator Du<T1, T2>(T2 value) => new(value);

	private T Get<T>() => Du.Get<T>(managed, in unmanaged);

	public TResult Match<TResult>(Func<T1, TResult> f1, Func<T2, TResult> f2) => GetIndex() switch
	{
		1 => f1(Get<T1>()),
		2 => f2(Get<T2>()),
		_ => throw new InvalidInstanceException()
	};

	public void Switch(Action<T1> a1, Action<T2> a2)
	{
		switch (GetIndex())
		{
			case 1: a1(Get<T1>()); break;
			case 2: a2(Get<T2>()); break;
			default: throw new InvalidInstanceException();
		}
	}

	public static ImmutableArray<Type> Types { get; } = [
		typeof(T1),
		typeof(T2)
	];

	static Du<T1, T2> IDu<Du<T1, T2>>.Deserialize(ref Utf8JsonReader reader, JsonSerializerOptions? options)
	{
		if (JsonSerializer.TryDeserialize<T1>(ref reader, options, out var v1) && v1 is not null)
			return v1;
		if (JsonSerializer.TryDeserialize<T2>(ref reader, options, out var v2) && v2 is not null)
			return v2;
		throw new JsonException("No match was found for converting the JSON into a " + typeof(Du<T1, T2>).NameWithGenericArguments);
	}

	void IDu<Du<T1, T2>>.Serialize(Utf8JsonWriter writer, JsonSerializerOptions? options)
	{
		switch (GetIndex())
		{
			case 1: JsonSerializer.Serialize(writer, Get<T1>(), options); break;
			case 2: JsonSerializer.Serialize(writer, Get<T2>(), options); break;
			default: throw new InvalidInstanceException();
		}
	}

	public override String ToString()
	{
		return Match(ToStr, ToStr);
		static String ToStr<T>(T x) => x?.ToString() ?? String.Empty;
	}

	public override Int32 GetHashCode()
	{
		return Match(GetHc, GetHc);
		static Int32 GetHc<T>(T x) => x?.GetHashCode() ?? 0;
	}

	public override Boolean Equals([NotNullWhen(true)] Object? obj) => obj switch
	{
		null => false,
		Du<T1, T2> du => Equals(du),
		_ => base.Equals(obj)
	};

	public Boolean Equals(Du<T1, T2> other) => (GetIndex(), other.GetIndex()) switch
	{
		(1, 1) => Equals<T1>(other),
		(2, 2) => Equals<T2>(other),
		_ => false
	};

	private Boolean Equals<T>(Du<T1, T2> other) => (Get<T>(), other.Get<T>()) switch
	{
		(null, null) => true,
		(null, not null) or (not null, null) => false,
		var (x, y) => x.Equals(y)
	};

	public static Boolean operator ==(Du<T1, T2> left, Du<T1, T2> right) => left.Equals(right);

	public static Boolean operator !=(Du<T1, T2> left, Du<T1, T2> right) => !(left == right);
}