namespace NickStrupat;

public interface ITypeVisitor<TRefParam> where TRefParam : allows ref struct
{
	void Initialize<TDu>() where TDu : IDu {}
	Boolean VisitType<T>(ref TRefParam refParam) where T : notnull;
}

public interface ITypeVisitor
{
	void Initialize<TDu>() where TDu : IDu {}
	Boolean VisitType<T>() where T : notnull;
}