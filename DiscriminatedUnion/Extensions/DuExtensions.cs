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
			_ = du.Visit<TVisitor, None>(ref visitor);
		}

		public void Visit<T>(Action<T> action) where T : notnull
		{
			du.Visit(new ActionVisitor<T>(action));
		}

		public TResult Visit<TVisitor, TResult>(out TVisitor visitor) where TVisitor : IVisitor<TResult>, new()
		{
			visitor = new();
			return du.Visit<TVisitor, TResult>(ref visitor);
		}

		public void Serialize(Utf8JsonWriter writer, JsonSerializerOptions options) => du.Visit(new JsonSerializerVisitor(writer, options));

		public Boolean Equals<T>(T other) where T : notnull =>
			du.TryPick(out T? matched) && EqualityComparer<T>.Default.Equals(matched, other);

		public Boolean TryPick<T>([NotNullWhen(true)] out T? matched) where T : notnull
		{
			var visitor = new TryPickVisitor<T>();
			var result = du.Visit<TryPickVisitor<T>, Boolean>(ref visitor);
			matched = visitor.Picked!;
			return result;
		}
	}

	extension(IDu du)
	{
		public Boolean TryPick<T>([NotNullWhen(true)] out T? matched) where T : notnull
		{
			var visitor = new TryPickVisitor<T>();
			var result = du.Visit<TryPickVisitor<T>, Boolean>(ref visitor);
			matched = visitor.Picked!;
			return result;
		}

		public T Pick<T>() where T : notnull
		{
			if (du.TryPick<IDu, T>(out var matched))
				return matched;
			throw new InvalidOperationException($"The discriminated union does not hold an instance of type {typeof(T).FullName}.");
		}
	}

	// public static Boolean Equals(this (IDu du1, IDu du2) pair)
	// {
	// 	var (du1, du2) = pair;
	//
	// }
}