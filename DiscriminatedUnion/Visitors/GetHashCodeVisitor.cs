namespace NickStrupat;

internal readonly struct GetHashCodeVisitor : IVisitor<Int32>
{
	Int32 IVisitor<Int32>.Visit<T>(T value) => value.GetHashCode();
}