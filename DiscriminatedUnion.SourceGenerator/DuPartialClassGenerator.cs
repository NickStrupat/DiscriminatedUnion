using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DiscriminatedUnion.SourceGenerator;

[Generator]
public class DuPartialClassGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var enumsToGenerate = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (s, _) => s is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
				transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)
			)
			.Where(static m => m is not null);

		// Generate source code for each Du found
		context.RegisterSourceOutput(enumsToGenerate, static (spc, source) => Execute(source, spc));
	}

	private static DuToGenerate? GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx)
	{
		var classDeclarationSyntax = (ClassDeclarationSyntax)ctx.Node;

		foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
		foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
		{
			if (ctx.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
				continue; // weird, we couldn't get the symbol, ignore it

			if (attributeSymbol.ContainingType.ToDisplayString().StartsWith("NickStrupat.DuAttribute<"))
				return GetDuToGenerate(ctx.SemanticModel, classDeclarationSyntax, attributeSymbol.ContainingType);
		}

		return null;
	}

	static DuToGenerate? GetDuToGenerate(SemanticModel semanticModel,
		SyntaxNode classDeclarationSyntax,
		INamedTypeSymbol attributeType)
	{
		if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
			return null;

		String className = classSymbol.Name;
		String? @namespace = classSymbol.ContainingNamespace is { IsGlobalNamespace: false } gns ? gns.ToDisplayString() : null;

		var nestedClasses = new List<String>();
		var currentType = classSymbol.ContainingType;
		while (currentType is not null)
		{
			nestedClasses.Insert(0, currentType.Name);
			currentType = currentType.ContainingType;
		}

		var typeNames = new List<String>();

		foreach (var ta in attributeType.TypeArguments)
		{
			typeNames.Add(ta.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
		}

		return new DuToGenerate(className, typeNames, @namespace, nestedClasses);
	}

	private static void Execute(DuToGenerate? source, SourceProductionContext spc)
	{
		if (source is not { } value)
			return;

		// generate the source code and add it to the output
		var result = GenerateExtensionClass(value);

		// Create a separate partial class file for each enum
		var fileName = value.NestedClasses.Count > 0 ? String.Join(".", value.NestedClasses) + "." : String.Empty;
		spc.AddSource($"DuGenerated.{value.Namespace ?? "global"}.{fileName}{value.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
	}

	private static String GenerateExtensionClass(DuToGenerate du2g)
	{
		var typeNames = String.Join(", ", du2g.TypeNames);
		var ctors = String.Join("\n\t",
			du2g.TypeNames.Select((tn, i) =>
				$"public {du2g.Name}({tn} instance{i + 1}) => du = new(instance{i + 1});"));
		var convOps = String.Join("\n\t",
			du2g.TypeNames.Select(tn => $"public static implicit operator {du2g.Name}({tn} value) => new(value);"));
		var funcParams = String.Join(", ", du2g.TypeNames.Select((tn, i) => $"Func<{tn}, TResult> f{i + 1}"));
		var funcArgs = String.Join(", ", du2g.TypeNames.Select((_, i) => $"f{i + 1}"));
		var actionParams = String.Join(", ", du2g.TypeNames.Select((tn, i) => $"Action<{tn}> a{i + 1}"));
		var actionArgs = String.Join(", ", du2g.TypeNames.Select((_, i) => $"a{i + 1}"));
		var acceptTypesBody = String.Join("\n\t\t", du2g.TypeNames.Select(tn => $"if (visitor.VisitType<{tn}>(ref refParam)) return;"));

		var classBody =
			$$"""
			[JsonConverter(typeof(Converter))]
			sealed partial class {{du2g.Name}} : IDu<{{du2g.Name}}>, IEquatable<{{du2g.Name}}> {
				private readonly Du<{{typeNames}}> du;
				private {{du2g.Name}}(Du<{{typeNames}}> du) => this.du = du;

				{{ctors}}

				{{convOps}}

				public static void AcceptTypes<TTypeVisitor, TRefParam>(ref TTypeVisitor visitor, ref TRefParam refParam)
				where TTypeVisitor : ITypeVisitor<TRefParam>
				where TRefParam : allows ref struct
				{
					visitor.Initialize({{du2g.TypeNames.Count}});
					{{acceptTypesBody}}
				}

				public static Boolean TryCreate<T>(T value, out {{du2g.Name}} du)
				{
					if (Du<{{typeNames}}>.TryCreate(value, out var innerDu))
					{
						du = new(innerDu);
						return true;
					}
					du = default!;
					return false;
				}

				public TResult Accept<TVisitor, TResult>(TVisitor visitor) where TVisitor : IVisitor<TResult> => Accept<TVisitor, TResult>(ref visitor);
				public TResult Accept<TVisitor, TResult>(ref TVisitor visitor) where TVisitor : IVisitor<TResult> => du.Accept<TVisitor, TResult>(ref visitor);

				public TResult Match<TResult>({{funcParams}}) => du.Match({{funcArgs}});
				public void Switch({{actionParams}}) => du.Switch({{actionArgs}});

				public static ImmutableArray<Type> Types => Du<{{typeNames}}>.Types;

				public override Int32 GetHashCode() => du.GetHashCode();
				public override Boolean Equals(Object? obj) => du.Equals(obj);
				public Boolean Equals({{du2g.Name}}? other) => other is not null && du.Equals(other.du);

				private sealed class Converter : JsonConverter<{{du2g.Name}}> {
					public override Boolean HandleNull => true;

					public override {{du2g.Name}} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
						new(JsonSerializer.Deserialize<Du<{{typeNames}}>>(ref reader, options));

					public override void Write(Utf8JsonWriter writer, {{du2g.Name}} value, JsonSerializerOptions options) =>
						JsonSerializer.Serialize(writer, value.du, options);
				}
			}
			""";

		var openingBrackets = String.Join("\n", du2g.NestedClasses.Select(nc => $"partial class {nc} {{"));
		var closingBrackets = String.Join("\n", Enumerable.Repeat("}", du2g.NestedClasses.Count));
		var @namespace = du2g.Namespace is not null ? $"namespace {du2g.Namespace};" : String.Empty;

		return
			$$"""
			using System.Collections.Immutable;
			using System.Runtime.CompilerServices;
			using System.Text.Json;
			using System.Text.Json.Serialization;
			using NickStrupat;
			
			#nullable enable

			{{@namespace}}

			{{openingBrackets}}
			{{classBody}}
			{{closingBrackets}}
			""";
	}
}

public readonly struct DuToGenerate(String name, List<String> typeNames, String? @namespace, List<String> nestedClasses)
{
	public String Name => name;
	public List<String> TypeNames => typeNames;
	public String? Namespace => @namespace;
	public List<String> NestedClasses => nestedClasses;
}