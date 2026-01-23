using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NickStrupat;

public sealed class InvalidInstanceException() : Exception("This instance was not constructed correctly.");

internal static class DU
{
    public static (Object?, UnmanagedStorage) Init<T>(ref T instance, Byte index) =>
        TypeInfoCache<T>.CanStoreInUnmanagedStorage
            ? (IndexObjects[index], AsUnmanaged(ref instance))
            : (instance, new() { _0 = index });

    public static T Get<T>(Object? managedReference, ref readonly UnmanagedStorage unmanagedBytes) =>
        TypeInfoCache<T>.CanStoreInUnmanagedStorage
            ? UnmanagedAs<T>(in unmanagedBytes)
            : (T)managedReference!;

    private static UnmanagedStorage AsUnmanaged<T>(ref T source)
    {
        UnmanagedStorage unmanagedStorage = default;
        Unsafe.WriteUnaligned(ref unmanagedStorage._0, source);
        return unmanagedStorage;
    }

    private static T UnmanagedAs<T>(in UnmanagedStorage source) => Unsafe.As<UnmanagedStorage, T>(ref Unsafe.AsRef(in source));

    private static class TypeInfoCache<T>
    {
        private static Boolean IsUnmanaged { get; } = !RuntimeHelpers.IsReferenceOrContainsReferences<T>();
        public static Boolean CanStoreInUnmanagedStorage { get; } = IsUnmanaged && Unsafe.SizeOf<T>() <= Unsafe.SizeOf<UnmanagedStorage>();
    }

    private static readonly Object[] IndexObjects = new Object[256];
    private static readonly FrozenDictionary<Object, Byte> IndexMap;

    static DU()
    {
        var indexMap = new Dictionary<Object, Byte>(IndexObjects.Length);
        for (var i = 0; i < IndexObjects.Length; i++)
            indexMap.Add(IndexObjects[i] = new Object(), (Byte)i);
        IndexMap = indexMap.ToFrozenDictionary();
    }

    public static Boolean TryGetIndex(Object obj, out Byte index) => IndexMap.TryGetValue(obj, out index);
    public static Object GetIndexObject(Byte index) => IndexObjects[index];
}

// public readonly struct DU<T1>
// {
//     private readonly Byte index;
//     private readonly UnmanagedStorage unmanagedBytes;
//     private readonly Object? managedReference;
//     public DU(T1 value) => (managedReference, index) = (value, 1);
// }

internal interface IDU<TDU> : IDU where TDU : IDU<TDU>
{
    //static abstract TDU Create(Object? instance);
    static abstract TDU VisitTypes<TTypeVisitor>(TTypeVisitor visitor) where TTypeVisitor : ITypeVisitor, allows ref struct;
    static abstract TDU TryDeserialize(ref Utf8JsonReader reader, JsonSerializerOptions? options);
}

internal interface IDU
{
    void Visit<TVisitor>(TVisitor visitor) where TVisitor : IVisitor;
    static abstract ImmutableArray<Type> Types { get; }
}

public interface ITypeVisitor
{
    Boolean Visit<T>(out T? value);
}

public interface IVisitor
{
    void Visit<T>(T value);
}

[JsonConverter(typeof(DUConverter))]
public readonly struct DU<T1, T2>
    : IEquatable<DU<T1, T2>>
    , IDU<DU<T1, T2>>
{
    private readonly Byte index => managedReference is {} mr && DU.TryGetIndex(mr, out var i) ? i : unmanagedStorage._0;
    private readonly UnmanagedStorage unmanagedStorage;
    private readonly Object? managedReference;

    public DU(T1 instance) => (managedReference, unmanagedStorage) = DU.Init(ref instance, 1);
    public DU(T2 instance) => (managedReference, unmanagedStorage) = DU.Init(ref instance, 2);
    // public DU(Object? instance) => (managedReference, unmanagedStorage) = instance switch
    // {
    //     T1 t1 => DU.Init(ref t1, 1),
    //     T2 t2 => DU.Init(ref t2, 2),
    //     _ => throw new InvalidOperationException()
    // };

    public static implicit operator DU<T1, T2>(T1 value) => new(value);
    public static implicit operator DU<T1, T2>(T2 value) => new(value);

    private T Get<T>() => DU.Get<T>(managedReference, in unmanagedStorage);

    public TResult Match<TResult>(Func<T1, TResult> f1, Func<T2, TResult> f2) => index switch
    {
        1 => f1(Get<T1>()),
        2 => f2(Get<T2>()),
        _ => throw new InvalidInstanceException()
    };

    public void Switch(Action<T1> a1, Action<T2> a2)
    {
        switch (index)
        {
            case 1: a1(Get<T1>()); break;
            case 2: a2(Get<T2>()); break;
            default: throw new InvalidInstanceException();
        }
    }

    // public static DU<T1, T2> Create(Object? instance) => new(instance);

    public void Visit<TVisitor>(TVisitor visitor) where TVisitor : IVisitor
    {
        switch (index)
        {
            case 1: visitor.Visit(Get<T1>()); break;
            case 2: visitor.Visit(Get<T2>()); break;
            default: throw new InvalidInstanceException();
        }
    }

    public static ImmutableArray<Type> Types { get; } = [typeof(T1), typeof(T2)];

    static DU<T1, T2> IDU<DU<T1, T2>>.VisitTypes<TVisitor>(TVisitor visitor)
    {
        if (visitor.Visit<T1>(out var v1))
            return new(v1!);
        if (visitor.Visit<T2>(out var v2))
            return new(v2!);
        throw new("ruh roh");
    }

    static DU<T1, T2> IDU<DU<T1, T2>>.TryDeserialize(ref Utf8JsonReader reader, JsonSerializerOptions? options)
    {
        if (JsonSerializer.TryDeserialize<T1>(ref reader, options, out var v1))
            return new(v1!);
        if (JsonSerializer.TryDeserialize<T2>(ref reader, options, out var v2))
            return new(v2!);
        throw new("ruh roh");
    }

    // public void Switch(Action<T1> a1, Action<T2> a2) => _ = Match(A2F(a1), A2F(a2));
    //
    // private static Func<T, Byte> A2F<T>(Action<T> action) => x => { action(x); return 0; };
    public Boolean Equals(DU<T1, T2> other) => (index, other.index) switch
    {
        (1, 1) => Equals<T1>(other),
        (2, 2) => Equals<T2>(other),
        _ => false
    };

    private Boolean Equals<T>(DU<T1, T2> other) => (Get<T>(), other.Get<T>()) switch
    {
        (null, null) => true,
        (null, not null) or (not null, null) => false,
        var (x, y) => x.Equals(y)
    };
}