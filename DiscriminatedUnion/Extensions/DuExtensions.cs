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
}