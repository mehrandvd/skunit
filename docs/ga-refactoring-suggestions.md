# GA Refactoring Suggestions (Breaking Changes Allowed)

This document proposes API and design refactors to make skUnit clearer and easier to use before GA.

## 1) Renamings

| Current | Suggested | Why |
|---|---|---|
| `KernelAssertionParser` | `ChatAssertionParser` | Assertions are evaluated from chat responses, not Kernel-specific APIs. |
| `IKernelAssertion.cs` (file name) | `IChatAssertion.cs` | File name should match contained type (`IChatAssertion`). |
| `HasConditionAssertion` | `SemanticConditionAssertion` | Matches markdown keyword (`SemanticCondition`) and user mental model. |
| `AreSimilarAssertion` | `SemanticSimilarityAssertion` | More readable than verb phrase; maps directly to `SemanticSimilar`. |
| `SemanticAgent` | `SemanticEvaluator` | This type evaluates conditions; it is not a user-facing conversational agent. |
| `SemanticAssert` | `SemanticAssertions` (or remove and fold into runner API) | Current name is confusingly close to xUnit assert style and runner assertions. |
| `SetJsonAssertText(...)` in `JsonCheckAssertion` | `ParseSpec(...)` | Method parses assertion spec; current name is legacy and ambiguous. |
| `SetJsonAssertText(...)` in `FunctionCallAssertion` | `ParseSpec(...)` | Same rationale as above, consistent naming. |
| `FunctionCallText` | `RawSpec` | Represents raw user spec (JSON or simple function name). |
| `ChatItem.Content` | `ChatItem.Text` (keep multimodal in `Contents`) | `Content` conflicts conceptually with `Contents`; better separation. |
| `ChatScenarioRunner.Initialize(...)` | `ConfigureDefaults(...)` (or remove globals entirely) | Better communicates process-wide default behavior. |
| `RunChatScenarioAsync(...)` extension methods | `ExecuteScenarioAsync(...)` | “Execute” is clearer for runtime behavior and avoids “Run/RunAsync” overload confusion. |

## 2) Modified Signatures

### 2.1 Add cancellation support and immutable input shapes

Replace mutable list-based signatures with read-only collections + cancellation token:

```csharp
Task RunAsync(
    ChatScenario scenario,
    IChatClient chatClient,
    IReadOnlyList<ChatMessage>? initialMessages = null,
    ScenarioRunOptions? options = null,
    CancellationToken cancellationToken = default);
```

Apply this to all `RunAsync(...)` overloads (single and collection, `IChatClient`, `AIAgent`, and delegate-based).

---

### 2.2 Replace delegate response contract

Current:

```csharp
Func<IList<ChatMessage>, Task<ChatResponse>> getAnswerFunc
```

Suggested:

```csharp
Func<IReadOnlyList<ChatMessage>, CancellationToken, ValueTask<ChatResponse>> getResponseAsync
```

Benefits:
- supports cancellation
- avoids requiring mutable history
- supports allocation-friendly fast path (`ValueTask`)

---

### 2.3 Remove or constrain global static initialization

Current:

```csharp
ChatScenarioRunner.Initialize(ILogger? logger = null, IChatClient? chatClient = null)
```

Suggested direction:
- Prefer constructor-only configuration:

```csharp
public ChatScenarioRunner(IChatClient assertionClient, ILogger<ChatScenarioRunner>? logger = null)
```

- If defaults are needed, use explicit options object passed per runner instance:

```csharp
public sealed class ChatScenarioRunnerOptions
{
    public IChatClient AssertionClient { get; init; } = default!;
    public ILogger? Logger { get; init; }
}
```

---

### 2.4 Strengthen scenario loading API

Current:

```csharp
public static List<TScenario> LoadFromText(string text)
public static Task<List<TScenario>> LoadFromResourceAsync(string resource, Assembly assembly)
```

Suggested:

```csharp
public static IReadOnlyList<TScenario> Parse(string markdown)
public static Task<IReadOnlyList<TScenario>> ParseFromResourceAsync(
    string resourceName,
    Assembly assembly,
    CancellationToken cancellationToken = default)
```

## 3) Design Changes

### 3.1 Remove success-shaped fallback path in `CreatePopulateAnswer`

Current implementation has a debug assert and fallback returning an empty assistant message when all providers are null. For GA, this should throw immediately with a clear error.

---

### 3.2 Parse all supported roles explicitly (`[SYSTEM]`, `[TOOL]`)

`ChatScenarioParser.PackBlock(...)` handles `SYSTEM` and `TOOL`, but parser heading detection does not currently match them. Add explicit heading parsing for these roles or remove role branches to avoid misleading API behavior.

---

### 3.3 Introduce one public execution entrypoint

Current surface has many overloads and extension methods (`RunAsync`, `RunChatScenarioAsync`) that overlap. For GA, expose one canonical entrypoint pattern and keep convenience overloads minimal.

---

### 3.4 Separate parsing model from runtime model

`ChatItem` currently carries both parse-time and runtime concerns (`Contents`, text fallback property, assertions). Consider:
- `ScenarioDocument` (parser output)
- `ScenarioStep` (runtime execution model)

This makes multimodal behavior and migration safer.

---

### 3.5 Normalize assertion naming and docs output

Parser supports both `CHECK` and `ASSERT`, but runtime logs always print `ASSERT`. Pick one canonical term (recommended: `ASSERT`) and keep parser aliases for backward compatibility.

---

### 3.6 Improve function-call assertion contract

`FunctionCallAssertion` currently validates against response message content with late-bound conversions. For GA:
- use strongly-typed spec model (`FunctionCallAssertionSpec`)
- parse once with explicit validation errors
- support configurable match strategy (`AnyCall`, `LastCall`, `ExactSingleCall`)

## 4) Recommended GA Rollout Strategy

1. Introduce new names/signatures in `vNext` with `[Obsolete]` shims for one pre-GA cycle.
2. Update docs and samples to only use new canonical APIs.
3. Remove obsolete surface at GA cut.

