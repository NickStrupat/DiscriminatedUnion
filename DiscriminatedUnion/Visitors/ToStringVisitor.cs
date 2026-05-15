namespace DiscriminatedUnion.Visitors;

internal readonly struct ToStringVisitor : IVisitor<String>
{
	String IVisitor<String>.Visit<T>(T value) => value.ToString() ?? String.Empty;
}