# skUnit
[![Build and Deploy](https://github.com/mehrandvd/skUnit/actions/workflows/build.yml/badge.svg)](https://github.com/mehrandvd/skUnit/actions/workflows/build.yml)
[![NuGet version (skUnit)](https://img.shields.io/nuget/v/skUnit.svg?style=flat)](https://www.nuget.org/packages/skUnit/)
[![NuGet downloads](https://img.shields.io/nuget/dt/skUnit.svg?style=flat)](https://www.nuget.org/packages/skUnit)
[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/mehrandvd/skunit)

**skUnit** is a semantic testing framework for .NET — write AI tests in **plain Markdown**, run them with **xUnit, NUnit, or MSTest**.

Test anything that talks to AI:
- **AI Agents** (Microsoft Agent Framework, A2A)
- **IChatClient** (Microsoft Extensions AI)
- **MCP servers** (Model Context Protocol)
- **Custom AI integrations**

```bash
dotnet add package skUnit
```

## Quick Start

Imagine you have an `AIAgent` configured to answer questions about a bank account. Here's a test scenario in Markdown, `balance-test.md`:

````markdown
# SCENARIO Balance Test

## [USER]
What is the account balance for John Doe?

## [ASSISTANT]

### ASSERT SemanticCondition
The answer mentions that the account balance is $1,234.56.
````

And the C# test:

```csharp
[Fact]
public async Task SimpleTest()
{
    var markdown = File.ReadAllText("balance-test.md");
    var scenario = ChatScenario.Parse(markdown);

    await agent.RunAsync(scenario);
}
```

That's it. skUnit drives the conversation, calls your AI, and **semantically verifies** the response.

> **Tip — Parameterized scenarios:** Use `{placeholders}` in your Markdown and replace them with C# string manipulation to run the same scenario with different inputs.
>
> ````markdown
> ## [USER]
> What is the account balance for {CustomerName}?
>
> ## [ASSISTANT]
>
> ### ASSERT SemanticCondition
> The answer mentions that the account balance is {CustomerBalance}.
> ````

## Key Features

### 1. Function Call Assertion

Verify that your agent calls the **right tool with the right arguments**:

````markdown
# SCENARIO Balance Test

## [USER]
What is the account balance for John Doe?

## [ASSISTANT]

### ASSERT SemanticCondition
The answer mentions that the account balance is $1,234.56.

### ASSERT FunctionCall
{
  "function_name": "get-account-balance"
}
````

You can also assert on the **arguments** passed to the function, using exact or semantic matching:

```json
{
  "function_name": "get-account-balance",
  "arguments": {
    "accountHolder": ["Equals", "John Doe"]
  }
}
```

```json
{
  "function_name": "get-account-balance",
  "arguments": {
    "accountHolder": ["SemanticSimilar", "John Doe"]
  }
}
```

> See [ASSERT JsonCheck](docs/check-statements-spec.md#assert-jsoncheck) for full JSON schema validation.

---

### 2. Multi-Turn Conversations

Test **realistic back-and-forth conversations** — each turn gets its own assertions:

````markdown
# SCENARIO Multi-Turn Balance Test

## [USER]
What is the account balance for John Doe?

## [ASSISTANT]
The account balance for John Doe is $1,234.56.

### ASSERT SemanticCondition
The answer mentions that the account balance is $1,234.56.

## [USER]
What about his credit score?

## [ASSISTANT]
His credit score is 750.

### ASSERT SemanticCondition
It mentions that the credit score is 750.
````

![skUnit Chat Scenario Structure](https://github.com/mehrandvd/skunit/assets/5070766/156b0831-e4f3-4e4b-b1b0-e2ec868efb5f)

Each `[ASSISTANT]` turn can carry **multiple ASSERT statements** to verify different aspects of the response independently.

---

### 3. MCP Server Testing

Test [Model Context Protocol](https://modelcontextprotocol.io/) servers end-to-end — same Markdown format, no extra APIs to learn:

````markdown
# SCENARIO MCP Time Server

## [USER]
What time is it?

## [ASSISTANT]
It's currently 2:30 PM PST

### ASSERT FunctionCall
{
  "function_name": "current_time"
}

### ASSERT SemanticCondition
It mentions a specific time
````

Wire up your MCP server in the test setup:

```csharp
var mcp = await McpClientFactory.CreateAsync(clientTransport);
var tools = await mcp.ListToolsAsync();

var baseChatClient = // your IChatClient (Azure OpenAI, OpenAI, etc.)

var agent = baseChatClient.AsAgent(
    instructions: "You are a helpful assistant that can answer questions and call tools.",
    tools: tools.ToArray());

await agent.ExecuteScenarioAsync(scenario, assertionClient: baseChatClient);
// assertionClient = the AI that evaluates responses semantically
// agent           = the system under test that actually calls tools
```

> Need to test **multiple MCP servers** together? Merge their tool lists before creating the agent — see [MCP Testing Guide](docs/mcp-testing-guide.md).

---

### 4. Hallucination Mitigation with `ScenarioRunOptions`

LLM outputs vary between runs. **Don't let a single flaky response break your build.**

Run each scenario multiple times and require a minimum number of passes:

```csharp
var options = new ScenarioRunOptions
{
    TotalRuns = 3,           // Run the scenario 3 times
    RequiredSuccessRuns = 2  // At least 2 must pass
};

await agent.ExecuteScenarioAsync(scenario, assertionClient: baseChatClient, options: options);
```

**Recommended thresholds:**

| Scenario type | TotalRuns | RequiredSuccessRuns |
|---|---|---|
| Deterministic / low-temp | 1 | 1 |
| Function / tool invocation | 3 | 2 |
| Creative generation | 5 | 3 |
| Critical CI gating | 5 | 4 |

When a scenario falls below the threshold, you get a clear failure message:
```
Only 2 of 5 runs passed, which is below the required success runs of 4.
```

> See [Scenario Run Options](docs/scenario-run-options.md) for the full guide.

---

## Setup

```csharp
public class BankAccountTests
{
    private readonly ChatScenarioRunner ScenarioRunner;
    private readonly IChatClient systemUnderTestClient;

    public BankAccountTests(ITestOutputHelper output)  // xUnit
    {
        var assertionClient = new AzureOpenAIClient(endpoint, credential)
            .GetChatClient(deploymentName)
            .AsIChatClient();

        ScenarioRunner = new ChatScenarioRunner(assertionClient, output.WriteLine);
    }

    [Fact]
    public async Task TestBalance()
    {
        var agentUnderTest = /* the client/agent you are testing */
        var scenarios = ChatScenario.Parse(File.ReadAllText("balance-test.md"));
        await ScenarioRunner.RunAsync(scenarios, systemUnderTestClient);
    }
}
```

Two clients, two roles:
- **`assertionClient`** — evaluates semantic assertions (`SemanticCondition`, `SemanticSimilar`, etc.)
- **`agentUnderTest`** — the agent/client whose behavior you are testing

> **MSTest / NUnit:** Replace `ITestOutputHelper.WriteLine` with `TestContext.WriteLine`. Everything else stays the same. See [Test Framework Integration](docs/test-framework-integration.md).

---

## Documentation

| Doc | Description |
|---|---|
| [Chat Scenario Spec](docs/chat-scenario-spec.md) | Complete guide to writing scenarios |
| [ASSERT Statement Spec](docs/check-statements-spec.md) | All available assertion types |
| [Test Framework Integration](docs/test-framework-integration.md) | xUnit, MSTest, NUnit setup |
| [MCP Testing Guide](docs/mcp-testing-guide.md) | Testing MCP servers |
| [Multi-Modal Support](docs/multi-modal-support.md) | Images and other media |
| [Scenario Run Options](docs/scenario-run-options.md) | Multi-run hallucination mitigation |

## Examples

See the `/demos` folder for complete, runnable projects:
- **[Demo.MoodyChef](demos/Demo.MoodyChef)**: Multi-turn chat testing with semantic assertions
- **[Demo.TddRepl](demos/Demo.TddRepl)**: Interactive chat application testing
- **[Demo.TddMcp](demos/Demo.TddMcp)**: MCP server integration testing
- **[Demo.TddShop](demos/Demo.TddShop)**: Complex multi-scenario chat testing

## Requirements

- **.NET 8.0** or higher
- An **AI provider** (Azure OpenAI, OpenAI, Anthropic, ...) for semantic assertions
- A **test framework** of your choice — xUnit, NUnit, or MSTest

## Contributing

Contributions are welcome! Check [open issues](https://github.com/mehrandvd/skunit/issues) or submit a PR.
