using static System.Reflection.BindingFlags;

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

	internal static TField? GetFieldValue<TField>(this Object obj, String fieldName)
	{
		ArgumentNullException.ThrowIfNull(obj);
		var field = obj.GetType().GetField(fieldName, Instance | Public | NonPublic);
		if (field is null)
			throw new ArgumentException($"Field '{fieldName}' not found in type '{obj.GetType()}'.");
		return (TField?)field.GetValue(obj);
	}
}