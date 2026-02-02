namespace NickStrupat;

public static class EnumerableExtensions
{
	extension(IEnumerable<String> enumerable)
	{
		public String Join(String separator) => String.Join(separator, enumerable);
		public String Join(Char separator) => String.Join(separator, enumerable);
		public String Join() => String.Join(String.Empty, enumerable);
	}
}