using System.Collections.Immutable;
using System.Diagnostics;
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
		// const String UnsafeAccessor =
		// 	"""
		// 	[UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Deserialize")]
		// 	static extern ref String Name(Foo @this);
		// 	""";
		// context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
		// 	"EnumExtensionsAttribute.g.cs",
		// 	SourceText.From(UnsafeAccessor, Encoding.UTF8)));

		IncrementalValuesProvider<DuToGenerate?> enumsToGenerate = context.SyntaxProvider
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

	static DuToGenerate? GetDuToGenerate(SemanticModel semanticModel, SyntaxNode classDeclarationSyntax, INamedTypeSymbol attributeType)
	{
		if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
			return null;

		String className = classSymbol.Name;
		String @namespace = classSymbol.ContainingNamespace?.ToDisplayString() ?? String.Empty;

		var typeNames = new List<String>();

		foreach (var ta in attributeType.TypeArguments)
		{
			typeNames.Add(ta.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
		}

		return new DuToGenerate(className, typeNames, @namespace);
	}

	private static void Execute(DuToGenerate? source, SourceProductionContext spc)
	{
		if (source is not { } value)
			return;

		// generate the source code and add it to the output
		var result = GenerateExtensionClass(value);

		// Create a separate partial class file for each enum
		spc.AddSource($"DuBase.{value.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
	}

	private static String GenerateExtensionClass(DuToGenerate du2g)
	{
		var typeNames = String.Join(", ", du2g.TypeNames);
		var ctors = String.Join("\n\t", du2g.TypeNames.Select((tn, i) => $"public {du2g.Name}({tn} instance{i + 1}) => du = new(instance{i + 1});"));
		var convOps = String.Join("\n\t", du2g.TypeNames.Select(tn => $"public static implicit operator {du2g.Name}({tn} value) => new(value);"));
		var funcParams = String.Join(", ", du2g.TypeNames.Select((tn, i) => $"Func<{tn}, TResult> f{i + 1}"));
		var funcArgs = String.Join(", ", du2g.TypeNames.Select((_, i) => $"f{i + 1}"));
		var actionParams = String.Join(", ", du2g.TypeNames.Select((tn, i) => $"Action<{tn}> a{i + 1}"));
		var actionArgs = String.Join(", ", du2g.TypeNames.Select((_, i) => $"a{i + 1}"));
		return
			$$"""
			using System.Collections.Immutable;
			using System.Runtime.CompilerServices;
			using System.Text.Json;
			using System.Text.Json.Serialization;
			using NickStrupat;

			namespace {{du2g.Namespace}};

			[JsonConverter(typeof(Converter))]
			partial class {{du2g.Name}} : IDu<Du<{{typeNames}}>> {
				private readonly Du<{{typeNames}}> du;
				private {{du2g.Name}}(Du<{{typeNames}}> du) => this.du = du;

				{{ctors}}

				{{convOps}}

				public Boolean TryPick<T>(out T matched) where T : notnull => du.TryPick(out matched);
				public void Visit<T>(Action<T> action) where T : notnull => du.Visit(action);
				public TResult Match<TResult>({{funcParams}}) => du.Match({{funcArgs}});
				public void Switch({{actionParams}}) => du.Switch({{actionArgs}});

				static Du<{{typeNames}}> IDu<Du<{{typeNames}}>>.Deserialize(ref Utf8JsonReader reader, JsonSerializerOptions options) => Du<{{typeNames}}>.Deserialize(ref reader, options);
				void IDu<Du<{{typeNames}}>>.Serialize(Utf8JsonWriter writer, JsonSerializerOptions options) => du.Serialize(writer, options);
				public static ImmutableArray<Type> Types => Du<{{typeNames}}>.Types;

			 	private sealed class Converter : JsonConverter<{{du2g.Name}}> {
					public override Boolean HandleNull => true;

					public override {{du2g.Name}} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
						new(Du<{{typeNames}}>.Deserialize(ref reader, options));

					public override void Write(Utf8JsonWriter writer, {{du2g.Name}} value, JsonSerializerOptions options) =>
						value.du.Serialize(writer, options);
				}
			}
			""";
	}
}

public readonly struct DuToGenerate(String name, List<String> typeNames, String @namespace)
{
	public String Name => name;
	public List<String> TypeNames => typeNames;
	public String Namespace => @namespace;
}