using System.Text.Json;
using DiscriminatedUnion.Visitors;

namespace DiscriminatedUnion.Extensions;

public static class DuExtensions
{
	extension<TDu>(TDu du) where TDu : IDu
	{
		public void Visit<T>(Action<T> action) where T : notnull
		{
			du.Accept(new ActionVisitor<T>(action));
		}

		public TResult Visit<TVisitor, TResult>(out TVisitor visitor) where TVisitor : IVisitor<TResult>, new()
		{
			visitor = new();
			return du.Accept<TVisitor, TResult>(ref visitor);
		}

		public void Serialize(Utf8JsonWriter writer, JsonSerializerOptions options) => du.Accept(new JsonSerializerVisitor(writer, options));

		public Boolean Equals<T>(T other) where T : notnull =>
			du.TryPick(out T? matched) && EqualityComparer<T>.Default.Equals(matched, other);
	}

	// Terminal per-arm When: when T2 is None, this concrete overload wins over the generic When in DuWhenExtensions
	// via specific-beats-generic overload resolution. Throws on default; runs handler iff the held value is the T1 arm;
	// no-op when the held value is None (a previous When in the chain fired).
	public static None When<T1>(this Du<T1, None> du, Action<T1> handler)
		where T1 : notnull
	{
		var idx = Du.GetIndex(du.managed, in du.unmanaged);
		if (idx == 1) handler(Du.Get<T1>(du.managed, in du.unmanaged));
		return default;
	}
}