namespace NickStrupat;

public interface ITypeVisitor<TRefParam> where TRefParam : allows ref struct
{
	Boolean VisitType<T>(ref TRefParam refParam) where T : notnull;
}