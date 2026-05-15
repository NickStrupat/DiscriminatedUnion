# DiscriminatedUnion

A discriminated union (sum type) for .NET, focused on zero-allocation dispatch and a fixed 24-byte footprint regardless of arm count.

```csharp
Du<int, string> du = 42;
var label = du.Match(i => $"int: {i}", s => $"str: {s}");
```

## Install

```
dotnet add package DiscriminatedUnion
```

Targets `net10.0`. Namespace `NickStrupat`.

## Two usage modes

### 1. Anonymous union — `Du<T1, ..., Tn>` struct

```csharp
using NickStrupat;

Du<int, string> du = "hello";          // implicit conversion per arm

du.Switch(
    i => Console.WriteLine($"int {i}"),
    s => Console.WriteLine($"str {s}")
);

var len = du.Match(
    i => i.ToString().Length,
    s => s.Length
);

if (du.TryPick<string>(out var s)) { /* ... */ }
```

Arities `Du<T1>` through `Du<T1...T16>` are generated.

### 2. Named union — derive from `DuBase<...>`, source-generated partial

```csharp
public sealed partial class JsonValue : DuBase<JsonObject, JsonValue[], string, long, double, bool, None>;
```

The source generator emits the constructors, implicit conversions, equality, and JSON converter for the partial. `None` is included for representing `null` JSON.

## API surface

| | Returns | Exhaustive at compile time? |
| --- | --- | --- |
| `Match(f1, …, fn)` | `TResult` | Yes — wrong arity is a compile error |
| `Switch(a1, …, an)` | `void` | Yes |
| `TryPick<T>(out T?)` | `bool` | No (runtime check per call) |
| `TryCreate<T>(T value, out Du)` | `bool` | No (runtime check per call) |
| `Accept<TVisitor, TResult>(ref TVisitor)` | `TResult` | Visitor's `Visit<T>` is generic — for arm-agnostic operations |
| `Accept<TVisitor>(ref TVisitor)` | `void` | Same, side-effect-only |

`TryCreate<T>` runs two passes: exact `typeof(T) == typeof(Tn)` first, then `value is Tn` for assignability (so `Du<Animal, int>.TryCreate<Dog>(dog, …)` succeeds with leftmost-arm-wins on ambiguity). Returns false for null.

`default(Du<…>)` throws `InvalidInstanceException` on any operation rather than silently picking arm 0.

## Storage

```
struct Du<T1, ..., Tn>          // 24 bytes total
├─ UnmanagedStorage  (16 bytes) // inline payload for small unmanaged values
└─ object?            (8 bytes) // discriminator sentinel + reference for boxed/managed values
```

For each arm, at construction time:

| Arm value type | Storage strategy | Allocation |
| --- | --- | --- |
| Unmanaged ≤ 16 bytes (`int`, `Guid`, `decimal`, `DateTime`, …) | Bytes packed inline; reference slot holds a cached `Index` sentinel | None |
| Value type with references, or > 16 bytes | Wrapped in a `Box<T>` record | One per construction |
| Reference type | Reference stored directly | None |

The cached sentinel array (one per byte index) means the discriminator costs nothing on the inline-storage path.

## Visitor dispatch (zero allocation)

`Accept<TVisitor, TResult>` constrains `TVisitor` to a struct implementing `IVisitor<TResult>`. The JIT monomorphises the call — no virtual dispatch, no boxing of the visitor, no closure allocation:

```csharp
internal readonly struct ToJson(Utf8JsonWriter writer, JsonSerializerOptions options) : IVisitor
{
    void IVisitor.Visit<T>(T value) => JsonSerializer.Serialize(writer, value, options);
}

du.Accept(new ToJson(writer, options));
```

The library uses this pattern internally for `ToString`, `GetHashCode`, `Equals`, JSON serialization, and `TryPick`. `Match`/`Switch` allocate one closure per call — fine for ergonomics, use `Accept` on hot paths.

## JSON

Serialization works out of the box via the included `JsonConverter`:

```csharp
Du<int, string> du = 42;
var json = JsonSerializer.Serialize(du);                 // "42"
var back = JsonSerializer.Deserialize<Du<int, string>>(json);
```

Named unions (`DuBase`-derived) get their own generated `JsonConverter`.

## Comparison with other DU libraries

| | This lib | [OneOf](https://github.com/mcintyre321/OneOf) | [Dunet](https://github.com/domn1995/dunet) | [LanguageExt](https://github.com/louthy/language-ext) |
| --- | --- | --- | --- | --- |
| Shape | `struct`, fixed 24 bytes | `struct`, one field per arm (grows with arity) | `record class` per arm | `class` (Either/Option/etc.) |
| Allocation for value types | None (≤ 16 bytes) | None (lives in arm field) | One per construction | One per construction |
| Allocation for reference types | None | None | One per construction | One per construction |
| Max arms (anonymous) | 16 | 9 | unlimited | n/a (fixed types) |
| Named unions | `DuBase<…>` + source gen | `OneOfBase<…>` (class) | Partial record + source gen | n/a |
| `Match` exhaustiveness | Compile-time (signature) | Compile-time (signature) | Compile-time (signature) | Compile-time |
| Pattern-matching syntax | `Match(f1, f2)` | `Match(f1, f2)` | `record` patterns + extension `Match` | LINQ-style |
| Visitor escape hatch | Yes (struct, no-alloc) | No | No | No |
| JSON support | Built-in | Via `OneOf.Json` | Manual | Via `LanguageExt.Newtonsoft.Json` |
| Scope | Discriminated unions only | Discriminated unions only | Discriminated unions only | Full functional toolkit |

**When to pick this library:** value-type-heavy workloads where allocation pressure matters, hot paths that benefit from struct-visitor dispatch, or when you want a fixed footprint regardless of arity.

**When to pick OneOf:** broadest community/ecosystem, you're already on it, or you want one field per arm so debugging shows all slots.

**When to pick Dunet:** the data is naturally a record hierarchy (algebraic data types, AST nodes), you don't mind heap allocation, you prefer C# pattern-matching syntax over `Match` lambdas.

**When to pick LanguageExt:** you want an opinionated functional library beyond just unions (Either, Option, Reader/Writer/State, monadic LINQ, etc.).

## Limitations

- Custom `IVisitor<T>` implementations that internally dispatch on `typeof(T)` won't get a compile-time signal when you add a new arm. Use `Match`/`Switch` for per-arm logic; reserve `Accept` for arm-agnostic operations (serialize, hash, ToString).
- `TryPick<T>` chains and `TryCreate<T>` callers are runtime-checked — adding an arm won't break their call sites either.
- Up to 16 arms (configurable in the T4 template; raises generated assembly size).

## License

MIT