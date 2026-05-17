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
| `Pick<T>(out T?)` *(ext)* | residual `Du<…rest>?` | Yes — residual shape is checked by the compiler |
| `When<T>(Action<T>)` *(ext)* | residual `Du<…rest>?` | Yes |
| `\|` pipe operator | residual `Du<…rest>?` or `None?` | Yes — chain ends when all arms are stripped |
| `Else(Action<object>)` *(ext)* | `None?` (terminator) | n/a — catch-all |
| `TryPick<T>(out T?)` | `bool` | No (runtime check per call) |
| `TryCreate<T>(T value, out Du)` | `bool` | No (runtime check per call) |
| `Accept<TVisitor, TResult>(ref TVisitor)` | `TResult` | Visitor's `Visit<T>` is generic — for arm-agnostic operations |
| `Accept<TVisitor>(ref TVisitor)` | `void` | Same, side-effect-only |

`TryCreate<T>` runs two passes: exact `typeof(T) == typeof(Tn)` first, then `value is Tn` for assignability (so `Du<Animal, int>.TryCreate<Dog>(dog, …)` succeeds with leftmost-arm-wins on ambiguity). Returns false for null.

`default(Du<…>)` throws `InvalidInstanceException` on any operation rather than silently picking arm 0.

## Residual extraction — `Pick`, `When`, `|`, `Else`

`Pick`, `When`, and the `|` operator all strip one arm from a `Du`. The return type is a `Du<…>?` of the remaining arms (with a `None` arm appended once you're down to a single type, so the residual type doesn't degenerate into a bare `T?`). A null residual means the targeted arm matched; a non-null residual carries the value that didn't. Chaining is compile-time exhaustive — once every arm has been stripped, the residual type collapses to `None?`.

```csharp
using DiscriminatedUnion;
using DiscriminatedUnion.Extensions;

Du<int, string, double> du = "hit";

// Pick: out-param style
Du<int, double>? rest = du.Pick(out string? matched);   // matched = "hit", rest = null

// When: invokes handler on match, returns residual
Du<int, double>? rest2 = du.When(s => Console.WriteLine(s));

// Pipe operator: chain handlers, one per arm
None? done = du
    | (int    i) => Console.WriteLine($"int {i}")
    | (string s) => Console.WriteLine($"str {s}")
    | (double d) => Console.WriteLine($"dbl {d}");
```

`Else` and two `|` overloads terminate a chain with a catch-all:

```csharp
None? done = du
    | (int i)  => Console.WriteLine($"int {i}")
    | (Else e) => Console.WriteLine($"other: {e.Value}");   // boxed unhandled value

None? done2 = du
    | (int i) => Console.WriteLine(i)
    | ()      => Console.WriteLine("not an int");           // parameterless catch-all

// Or chain Else off a partial residual:
None? done3 = (du | (int i) => Console.WriteLine(i))
    .Else(value => Console.WriteLine($"other: {value}"));
```

All of these are also available on `DuBase<…>` subclasses and on `Du<…>?` (null propagates through the chain). `Du<T, None>` is recognized specially: the `None` arm is silently skipped by `|`/`Else`/parameterless handlers.

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
- `TryPick<T>` chains and `TryCreate<T>` callers are runtime-checked — adding an arm won't break their call sites either. `Pick`/`When`/`|` chains, by contrast, *are* compile-time exhaustive: the residual type changes when you add an arm, breaking stale call sites.
- `Else` and `|`-with-`Action<Else>` box value-type arms (the boxed value is exposed as `Else.Value`, typed `object`). Per-arm handlers (`Action<T>`) stay allocation-free.
- Up to 16 arms (configurable via `MaxDuTypes` in `Templates/Shared.ttinclude`; raises generated assembly size).

## License

MIT