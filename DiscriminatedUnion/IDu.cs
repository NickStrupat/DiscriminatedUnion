using System.Collections.Immutable;

namespace NickStrupat;

public interface IDu
{
	static abstract ImmutableArray<Type> Types { get; }
}