namespace NickStrupat;

public interface IDu<TDu> : IDu where TDu : IDu<TDu>
{
	static virtual Boolean AcceptTypes<TTypeVisitor, TRefParam>(
		TTypeVisitor visitor,
		ref TRefParam refArg
	)
		where TTypeVisitor : ITypeVisitor<TDu, TRefParam>
		where TRefParam : allows ref struct
	{
		return TDu.AcceptTypes(ref visitor, ref refArg);
	}

	static abstract Boolean AcceptTypes<TTypeVisitor, TRefParam>(ref TTypeVisitor visitor, ref TRefParam refArg)
		where TTypeVisitor : ITypeVisitor<TDu, TRefParam>
		where TRefParam : allows ref struct;

	static abstract Boolean TryCreate<T>(T value, out TDu du);
}