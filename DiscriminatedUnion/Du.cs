using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace NickStrupat;

internal static class Du
{
    public static (Object?, UnmanagedStorage) Init<T>(ref T instance, Byte index) =>
        TypeInfoCache<T>.CanStoreInUnmanagedStorage
            ? (Indexes.Span[index], AsUnmanaged(ref instance))
            : TypeInfoCache<T>.IsValueType
                ? (new Box<T>(instance), new UnmanagedStorage(index))
                : (instance, new UnmanagedStorage(index));

    public static T Get<T>(Object? managedReference, ref readonly UnmanagedStorage unmanagedBytes) =>
        TypeInfoCache<T>.CanStoreInUnmanagedStorage
            ? UnmanagedAs<T>(in unmanagedBytes)
            : TypeInfoCache<T>.IsValueType
                ? ((Box<T>)managedReference!).Value
                : (T)managedReference!;

    private sealed class Box<T>(T value)
    {
        public T Value = value;
    }

    private static UnmanagedStorage AsUnmanaged<T>(ref T source)
    {
        UnmanagedStorage unmanagedStorage = default;
        Unsafe.WriteUnaligned(ref unmanagedStorage._0, source);
        return unmanagedStorage;
    }

    private static T UnmanagedAs<T>(in UnmanagedStorage source) => Unsafe.As<UnmanagedStorage, T>(ref Unsafe.AsRef(in source));

    private static class TypeInfoCache<T>
    {
        public static Boolean IsValueType { get; } = typeof(T).IsValueType;
        private static Boolean IsUnmanaged { get; } = !RuntimeHelpers.IsReferenceOrContainsReferences<T>();
        public static Boolean CanStoreInUnmanagedStorage { get; } = IsUnmanaged && Unsafe.SizeOf<T>() <= Unsafe.SizeOf<UnmanagedStorage>();
    }

    static Du()
    {
        // Here we create a static lookup map of sentinel objects which are used to encode the type index so that we can
        // prevent boxing by directly storing small unmanaged types in the bytes of the unmanaged storage field.
        var array = new Index[256];
        for (var i = 0; i < array.Length; i++)
            array[i] = new Index((Byte)i);
        Indexes = array;
    }

    public sealed record Index(Byte Value);
    public static readonly ReadOnlyMemory<Index> Indexes;
}