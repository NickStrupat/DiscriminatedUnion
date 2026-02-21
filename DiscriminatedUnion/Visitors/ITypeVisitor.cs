namespace NickStrupat;

public interface ITypeVisitor<in TDu, TRefParam> where TDu : IDu<TDu> where TRefParam : allows ref struct
{
	Boolean VisitType<T>(ref TRefParam refArg, Func<T, TDu> duFunc) where T : notnull;
}