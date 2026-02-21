using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NickStrupat;

internal static class Du
{
	public static (Object?, UnmanagedStorage) Init<T>(ref T instance, Byte index) where T : notnull =>
		TypeInfoCache<T>.CanStoreInUnmanagedStorage
			? (Indexes[index], AsUnmanaged(ref instance))
			: TypeInfoCache<T>.IsValueType
				? (new Box<T>(instance), new UnmanagedStorage(index))
				: (instance, new UnmanagedStorage(index));

	public static T Get<T>(Object? managed, ref readonly UnmanagedStorage unmanagedBytes) where T : notnull =>
		TypeInfoCache<T>.CanStoreInUnmanagedStorage
			? UnmanagedAs<T>(in unmanagedBytes)
			: TypeInfoCache<T>.IsValueType
				? ((Box<T>)managed!).Value
				: (T)managed!;

	public static Byte GetIndexUnsafe(Object? managed, ref readonly UnmanagedStorage unmanaged) =>
		managed is Index index
			? index.Value
			: unmanaged._0;

	public static Byte GetIndex(Object? managed, ref readonly UnmanagedStorage unmanaged) =>
		GetIndexUnsafe(managed, in unmanaged) is var index && index > 0
			? index
			: throw new InvalidInstanceException();

	private sealed record Box<T>(T Value) where T : notnull;

	private static UnmanagedStorage AsUnmanaged<T>(ref T source)
	{
		UnmanagedStorage unmanagedStorage = default;
		Unsafe.WriteUnaligned(ref unmanagedStorage._0, source);
		return unmanagedStorage;
	}

	private static T UnmanagedAs<T>(in UnmanagedStorage source) => Unsafe.ReadUnaligned<T>(in source._0);

	private static class TypeInfoCache<T>
	{
		public static Boolean IsValueType { get; } = typeof(T).IsValueType;
		private static Boolean IsUnmanaged { get; } = !RuntimeHelpers.IsReferenceOrContainsReferences<T>();
		public static Boolean CanStoreInUnmanagedStorage { get; } = IsUnmanaged && Unsafe.SizeOf<T>() <= Unsafe.SizeOf<UnmanagedStorage>();
	}

	static Du()
	{
		// Here we create a static array of sentinel objects which are used to encode the type index so that we can
		// prevent boxing by directly storing small unmanaged types in the bytes of the unmanaged storage field.
		var array = new Index[256];
		for (var i = 0; i < array.Length; i++)
			array[i] = new Index((Byte)i);
		Indexes = ImmutableCollectionsMarshal.AsImmutableArray(array);
	}

	public sealed record Index(Byte Value);
	public static readonly ImmutableArray<Index> Indexes;

	public sealed class DebugView(IDu du)
	{
		private readonly IDu du = du; // hides the primary constructor parameter from the debugger display (at least in Rider)

		public Byte Index
		{
			get
			{
				var unmanaged = du.GetFieldValue<UnmanagedStorage>(nameof(Du<,>.unmanaged));
				return GetIndex(du.GetFieldValue<Object>(nameof(Du<,>.managed))!, in unmanaged);
			}
		}

		public Object Value
		{
			get
			{
				var visitor = new Visitor();
				return du.Accept<Visitor, Object>(ref visitor);
			}
		}

		private readonly struct Visitor : IVisitor<Object>
		{
			Object IVisitor<Object>.Visit<TValue>(TValue value) => value;
		}
	}
}