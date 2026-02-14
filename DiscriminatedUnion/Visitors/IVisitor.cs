namespace NickStrupat;

public interface IVisitor
{
	void Visit<T>(T value) where T : notnull;
}

public interface IVisitor<out TResult>
{
	TResult Visit<T>(T value) where T : notnull;
}