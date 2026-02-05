namespace NickStrupat;

public static class TypeExtensions
{
	extension(Type type)
	{
		public String NameWithGenericArguments =>
			type.Name.IndexOf('`') switch
			{
				-1 => type.Name,
				var i => $"{type.Name[..i]}<{type.GetGenericArguments().Select(x => x.Name).Join(", ")}>"
			};
	}
}