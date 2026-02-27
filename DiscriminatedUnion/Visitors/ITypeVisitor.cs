namespace NickStrupat;

public interface ITypeVisitor<TRefParam> where TRefParam : allows ref struct
{
	void Initialize(Int32 typeCount) {}
	Boolean VisitType<T>(ref TRefParam refParam) where T : notnull;
}