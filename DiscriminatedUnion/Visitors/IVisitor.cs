namespace NickStrupat;

public interface IVisitor<out TResult>
{
	TResult Visit<T>(T value) where T : notnull;
}