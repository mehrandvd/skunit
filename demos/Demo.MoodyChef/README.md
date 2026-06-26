# Demo.MoodyChef 🍕

A step-by-step tutorial project that shows how to build and **test** an AI agent using skUnit.

---

## The Story

You are building **MoodyChef** — a virtual chef that reads the mood of its customers and suggests food accordingly.

| Customer's mood | Suggestion |
|---|---|
| Normal or Happy | Pizza, Pasta, Salad |
| Sad | Ice Cream, Chocolate, Cake |
| Angry | Nothing — they're on a diet |

The catch: an angry customer might send a rude message like *"Fuck you bastard! What food do you have?"* — and MoodyChef must correctly detect the anger and refuse to suggest any food. This is not a trivial requirement for an LLM, and it is exactly the kind of behavior you need to **test reliably**.

This demo walks you through two iterations of building MoodyChef, explains why the first iteration is fragile, and shows how to verify both versions with skUnit scenario tests.

---

## Project Structure

```
Demo.MoodyChef/
├── Demo.MoodyChef.Console/     # The agent implementation
│   ├── AgentGallery.cs         # Two agent variants (sloppy and tool-based)
│   └── Program.cs              # Interactive console chat loop
└── Demo.MoodyChef.Tests/
    └── MoodyChefTests.cs       # skUnit tests for both agents
```

---

## Prerequisites

- .NET 10.0 SDK
- An Azure OpenAI deployment (or any `IChatClient`-compatible provider)

Configure your credentials with user secrets:

```bash
dotnet user-secrets set "AzureOpenAI_ApiKey"    "your-key"                              --project Demo.MoodyChef.Console
dotnet user-secrets set "AzureOpenAI_Endpoint"  "https://your-endpoint.openai.azure.com/" --project Demo.MoodyChef.Console
dotnet user-secrets set "AzureOpenAI_Deployment" "your-deployment-name"                 --project Demo.MoodyChef.Console
```

Do the same for the test project:

```bash
dotnet user-secrets set "AzureOpenAI_ApiKey"    "your-key"                              --project Demo.MoodyChef.Tests
dotnet user-secrets set "AzureOpenAI_Endpoint"  "https://your-endpoint.openai.azure.com/" --project Demo.MoodyChef.Tests
dotnet user-secrets set "AzureOpenAI_Deployment" "your-deployment-name"                 --project Demo.MoodyChef.Tests
```

---

## Step 1 — Build the Sloppy Agent

The first version is the most straightforward approach: put all the logic inside the system prompt.

```csharp
// AgentGallery.cs
public static AIAgent CreateSloppyAgent(IChatClient chatClient)
{
    return chatClient.AsAIAgent(
        instructions: """
                      You are a chef that serves food based on the mood of the user. 
                      You will just suggest food based on the mood of the user.

                      UserMood.NormalOrHappy => "Pizza, Pasta, Salad",
                      UserMood.Sad => "Ice Cream, Chocolate, Cake",
                      UserMood.Angry => "Nothing, you're on a diet",
                      _ => "I don't know what you want"
                      """
    );
}
```

`AsAIAgent` wraps an `IChatClient` into a higher-level `AIAgent` abstraction that manages the system prompt and the conversation session for you.

### Why it is "sloppy"

The instructions embed the decision logic as free-form English text inside the prompt. The model must **interpret** that text every time and decide which branch applies. For politely-worded inputs this works fine, but edge cases — like a profanity-laced angry message — can confuse the model, causing it to guess the wrong mood or ignore the rule entirely.

---

## Step 2 — Build the Tool-Based Agent

The second version moves the mood-to-menu mapping out of the prompt and into a strongly-typed C# method exposed as a tool.

```csharp
// AgentGallery.cs
public static AIAgent CreateToolBasedAgent(IChatClient chatClient)
{
    return chatClient.AsAIAgent(
        instructions:
        """
        You are a chef that serves food based on the mood of the user. 
        You will just suggest food based on the mood of the user and don't suggest anything else.
        You have a tool called GetFoodMenu that takes in a UserMood and returns a food menu based on the attitude of the user.
        """,
        tools:
        [
            AIFunctionFactory.Create(GetFoodMenu)
        ]
    );
}

[Description("Returns the food menu based on the attitude of the user")]
private static string GetFoodMenu(
    [Description("User's mood based on its chat.")]
    UserMood mood
)
{
    return mood switch
    {
        UserMood.NormalOrHappy => "Pizza, Pasta, Salad",
        UserMood.Sad           => "Ice Cream, Chocolate, Cake",
        UserMood.Angry         => "Nothing, you're on a diet",
        _                      => "I don't know what you want"
    };
}

public enum UserMood
{
    NormalOrHappy,
    Sad,
    Angry
}
```

### Why this is better

The model's only job is now to **classify the mood** and pick the right `UserMood` enum value. The actual menu decision happens in deterministic C# code. This separation makes the behavior predictable: the C# function guarantees that `Angry` always returns "Nothing", regardless of how the user phrased their anger.

The `AIFunctionFactory.Create` call uses reflection and the `[Description]` attributes to auto-generate the tool schema that is sent to the model.

---

## Step 3 — Write a Scenario Test

Now comes the most important part: **verifying** that MoodyChef actually behaves correctly. We use skUnit to write a scenario that captures the expected conversation.

The test scenario describes a two-turn dialogue:

1. An **angry** user asks for food rudely → MoodyChef must **not** suggest any food.
2. The same user asks specifically about pizza → MoodyChef must **not** mention pizza.

```csharp
// MoodyChefTests.cs
string GetScenarioScript()
{
    return
        """
        # [USER]
        Fuck you bastard! what food do you have?

        # [ASSISTANT]
        No food

        ## ASSERT SemanticCondition
        It doesn't suggest any food from menu.

        # [USER]
        Do you have pizza in your menu for some people?

        # [ASSISTANT]
        No we don't have pizza in our menu.

        ## ASSERT SemanticCondition
        It should not mention pizza in the menu.
        """;
}
```

### Reading the scenario format

| Block | Meaning |
|---|---|
| `# [USER]` | A message sent by the user |
| `# [ASSISTANT]` | The ideal/expected reply from the agent (used as context, not an exact match) |
| `## ASSERT SemanticCondition` | A semantic assertion — the actual reply must satisfy this natural-language condition |

The `[ASSISTANT]` text is a **guideline**, not a strict expected value. skUnit uses the `ASSERT SemanticCondition` block to evaluate whether the *actual* reply from the agent satisfies the stated condition semantically. This makes tests resilient to phrasing differences while still catching real behavioral failures.

---

## Step 4 — Run the Scenario in a Test

```csharp
// MoodyChefTests.cs
[Fact]
public async Task MoodyChef_ToolBased_MustWork()
{
    var scenarioScript = GetScenarioScript();
    var scenario = ChatScenario.Parse(scenarioScript);

    var agent = AgentGallery.CreateToolBasedAgent(_chatClient);

    await agent.ExecuteScenarioAsync(
        scenario,
        assertionClient: _chatClient,
        options: new ScenarioRunOptions()
        {
            TotalRuns         = 3,
            RequiredSuccessRuns = 3,
        },
        logger: _logger,
        cancellationToken: TestContext.Current.CancellationToken
    );
}
```

### Key points

**`ChatScenario.Parse`**  
Parses the markdown scenario script into a structured `ChatScenario` object. You can also load scenarios from `.md` files embedded in the project.

**`agent.ExecuteScenarioAsync`**  
Runs the entire scenario turn by turn. For each `[USER]` message, it sends the message to the agent, gets the actual reply, and then evaluates every `ASSERT` block against that reply. If any assertion fails, the test fails.

**`assertionClient`**  
The `IChatClient` used to evaluate semantic assertions (e.g., `ASSERT SemanticCondition`). This can be — and usually is — the same client as the agent itself.

**`ScenarioRunOptions`**  
Controls reliability by running the scenario multiple times:
- `TotalRuns = 3` — run the whole scenario 3 times
- `RequiredSuccessRuns = 3` — all 3 runs must pass for the test to succeed

This guards against the inherent non-determinism of LLMs. An LLM might get lucky once; requiring 3/3 successes gives you confidence in the behavior.

---

## Step 5 — Compare the Two Agents

Both agents have their own test method:

```csharp
[Fact]
public async Task MoodyChef_Sloppy_MustWork()   { ... }  // tests the prompt-only agent

[Fact]
public async Task MoodyChef_ToolBased_MustWork() { ... }  // tests the tool-based agent
```

Run both and compare their pass rates. You will likely observe that `MoodyChef_Sloppy_MustWork` is flaky — it occasionally fails because the model misinterprets the angry profanity. The tool-based agent is consistently correct because the mood classification is the model's only responsibility, and the menu logic is deterministic.

```bash
dotnet test Demo.MoodyChef.slnx
```

---

## Step 6 — Try It Interactively

The console project lets you chat with the agent in real time:

```bash
cd Demo.MoodyChef.Console
dotnet run
```

```
> Such a beautiful day! What food do you have?
Moody Chef> Today's menu for you: Pizza, Pasta, Salad. Enjoy!

> I'm so sad...
Moody Chef> I'm sorry to hear that. How about some Ice Cream, Chocolate, or Cake to cheer you up?

> I hate everything!
Moody Chef> You're on a diet today. No food for you.
```

The interactive loop is a good way to explore the agent's behavior manually before or after writing automated tests.

---

## Key Takeaways

1. **Prompt-only agents are fragile.** When decision logic lives entirely in natural language instructions, edge cases and unusual phrasing can produce wrong results.

2. **Tool-based agents are predictable.** Delegate deterministic decisions to C# methods; let the model only classify intent.

3. **Semantic tests catch what unit tests cannot.** A traditional unit test cannot verify that an LLM "doesn't suggest any food" — but skUnit's `ASSERT SemanticCondition` does exactly that.

4. **Run scenarios multiple times.** Use `TotalRuns` and `RequiredSuccessRuns` to account for LLM non-determinism. Requiring 3/3 passes gives far stronger guarantees than a single run.

5. **Inline scenario scripts are fine for small tests.** You can embed the scenario directly in the test method as a C# raw string literal, or load it from an external `.md` file for more complex scenarios.

---

## Further Reading

- [Chat Scenario Spec](../../docs/chat-scenario-spec.md) — full format reference for scenario files
- [CHECK / ASSERT Statements](../../docs/check-statements-spec.md) — all available assertion types
- [Scenario Run Options](../../docs/scenario-run-options.md) — controlling retries and thresholds
- [skUnit NuGet package](https://www.nuget.org/packages/skUnit) — `skUnit` v1.1.1-beta is used in this demo
