using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace NickStrupat;

internal static class Du
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

    static Du()
    {
        // Here we create a static lookup map of sentinel objects which are used to encode the type index so that we can
        // prevent boxing by directly storing small unmanaged types in the bytes of the unmanged storage field.
        var indexMap = new Dictionary<Object, Byte>(IndexObjects.Length);
        for (var i = 0; i < IndexObjects.Length; i++)
            indexMap.Add(IndexObjects[i] = new Object(), (Byte)i);
        IndexMap = indexMap.ToFrozenDictionary();
    }

    private static readonly Object[] IndexObjects = new Object[256];
    private static readonly FrozenDictionary<Object, Byte> IndexMap;

    public static Boolean TryGetIndex(Object obj, out Byte index) => IndexMap.TryGetValue(obj, out index);
    public static Object GetIndexObject(Byte index) => IndexObjects[index];
}