namespace DiscriminatedUnion;

public readonly struct Else
{
	public Object Value { get; }
	internal Else(Object value) => Value = value;
}
