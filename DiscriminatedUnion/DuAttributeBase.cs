namespace NickStrupat;

[AttributeUsage(AttributeTargets.Class)]
public class DuAttributeBase : Attribute
{
	private protected DuAttributeBase(Type[] types) => this.types = types;
	private readonly ReadOnlyMemory<Type> types;
	internal ReadOnlySpan<Type> Types => types.Span;
}