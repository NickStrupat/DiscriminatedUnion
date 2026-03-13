using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace NickStrupat;

public static class DuExtensions
{
	extension<TDu>(TDu du) where TDu : IDu
	{
		public void Visit<TVisitor>(TVisitor visitor) where TVisitor : IVisitor<None>
		{
			du.Visit(ref visitor);
		}

		public void Visit<TVisitor>(ref TVisitor visitor) where TVisitor : IVisitor<None>
		{
			_ = du.Accept<TVisitor, None>(ref visitor);
		}

		public void Visit<T>(Action<T> action) where T : notnull
		{
			du.Visit(new ActionVisitor<T>(action));
		}

		public TResult Visit<TVisitor, TResult>(out TVisitor visitor) where TVisitor : IVisitor<TResult>, new()
		{
			visitor = new();
			return du.Accept<TVisitor, TResult>(ref visitor);
		}

		public void Serialize(Utf8JsonWriter writer, JsonSerializerOptions options) => du.Visit(new JsonSerializerVisitor(writer, options));

		public Boolean Equals<T>(T other) where T : notnull =>
			du.TryPick(out T? matched) && EqualityComparer<T>.Default.Equals(matched, other);
	}
}