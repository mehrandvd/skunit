# skUnit
[![Build and Deploy](https://github.com/mehrandvd/skUnit/actions/workflows/build.yml/badge.svg)](https://github.com/mehrandvd/skUnit/actions/workflows/build.yml)
[![NuGet version (skUnit)](https://img.shields.io/nuget/v/skUnit.svg?style=flat)](https://www.nuget.org/packages/skUnit/)
[![NuGet downloads](https://img.shields.io/nuget/dt/skUnit.svg?style=flat)](https://www.nuget.org/packages/skUnit)
[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/mehrandvd/skunit)

**skUnit** is a semantic testing framework for .NET that makes it easy to test AI-powered applications using simple, readable Markdown scenarios.

Test anything that talks to AI:
- **AIAgents** (Microsoft Agent Framework, A2A)
- **IChatClient** (Microsoft Extensions AI)
- **MCP (Model Context Protocol) servers**
- **Custom AI integrations**

Write your tests in **Markdown**, run them with **any test framework** (xUnit, NUnit, MSTest), and get **live, readable results**.

## Quick Start

Imagine you have an `AIAgent` which is configured to answer questions about a bank account. You want to test that it correctly reports the account balance for a user named John Doe.


Here's a simple test scenario in Markdown, `balance-test.md`:

```markdown

# SCENARIO Balance Test

## [USER]
What is the account balance for John Doe?

## [ASSISTANT]

### ASSERT SemanticCondition
The answer mentions that the account balance is $1,234.56.
```

And here's how to test it with just a few lines of C#:

```csharp
[Fact]
public async Task SimpleTest()
{
    var markdown = File.ReadAllText("balance-test.md");
    var scenario = ChatScenario.Parse(markdown);

    await agent.RunAsync(scenario);
}
```

That's it! skUnit handles the conversation, calls your AI, and verifies the response makes sense.

It worths to mention that you can write parameterized scenarios like:

```markdown
# SCENARIO Balance Test

## [USER]
What is the account balance for {CustomerName}?

## [ASSISTANT]

### ASSERT SemanticCondition
The answer mentions that the account balance is {CustomerBalance}.
```

and use the power of C# to replace the placeholders with different values to test multiple cases.


But skUnit is much more than just a simple assertion framework. It supports **multi-turn conversations**, **JSON validation**, **function call testing**, and even **MCP server testing**. Let's explore some of the key features.

## Key Features

### 1. Function Call Assertion
You can assert that your AI calls the right functions (MCP maybe):

```markdown

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
```

You can even assert the parameters passed to the function:

```json
{
  "function_name": "get-account-balance",
  "arguments": {
    "accountHolder": ["Equals", "John Doe"]
  }
}
```

or even,

```json
{
  "function_name": "get-account-balance",
  "arguments": {
    "accountHolder": ["SemanticSimilar", "John Doe"]
  }
}
```

For full Json schema validation, see the [ASSERT JsonCheck](docs/check-statements-spec.md#assert-jsoncheck) documentation.

### 2. Multi-Turn Conversations

Test complex conversations with multiple exchanges:
```md
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
```

![skUnit Chat Scenario Structure](https://github.com/mehrandvd/skunit/assets/5070766/156b0831-e4f3-4e4b-b1b0-e2ec868efb5f)

Each scenario can contain multiple sub-scenarios (conversation turns), and each response can have multiple ASSERT statements to verify different aspects of the AI's behavior.

### 5. MCP Server Testing

Test [Model Context Protocol](https://modelcontextprotocol.io/) servers to ensure your tools work correctly:

```md
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
```

```csharp
// Setup MCP server testing
var mcp = await McpClientFactory.CreateAsync(clientTransport);
var tools = await mcp.ListToolsAsync();

var baseChatClient = // your base IChatClient (Azure OpenAI, OpenAI, etc.)

var agent = baseChatClient.AsAgent(
    instructions: "You are a helpful assistant that can answer questions and call tools.",
    tools: tools.ToArray());

// In your test class constructor:
await agent.ExecuteScenarioAsync(scenario, assertionClient: baseChatClient);
// assertionClient is the AI used to evaluate the agent's responses semantically, while the agent uses the tools to respond.
```

### 6. Mitigating Hallucinations with ScenarioRunOptions

LLM outputs can vary between runs. A single spurious response shouldn't fail your build if the model normally behaves correctly.

Use `ScenarioRunOptions` to execute each scenario multiple times and require a specific number of successful runs to pass. If `RequiredSuccessRuns` is not set, all runs must pass. This adds statistical robustness without eliminating genuine regressions.

```csharp
var options = new ScenarioRunOptions
{
    TotalRuns = 3,              // Run the whole scenario three times
    RequiredSuccessRuns = 2    // At least 2 of 3 runs must pass
};

await agent.ExecuteScenarioAsync(
    scenario,
    assertionClient: baseChatClient, 
    options: options);
```

Recommended starting points:
- Deterministic / low-temp prompts: `TotalRuns = 1`, `RequiredSuccessRuns = 1`
- Function / tool invocation: `TotalRuns = 3`, `RequiredSuccessRuns = 2`
- Creative generation: `TotalRuns = 5`, `RequiredSuccessRuns = 3`
- Critical CI gating: `TotalRuns = 5`, `RequiredSuccessRuns = 4`

Failure message example:
```
Only 2 of 5 runs passed, which is below the required success runs of 4.
```
Indicates a systematic issue (not just randomness) – investigate prompt, model settings, or assertions.

See full guide: [Scenario Run Options](docs/scenario-run-options.md)

### 7. Readable Markdown Scenarios

Your test scenarios are just **valid Markdown files** - easy to read, write, and review:

![Markdown Scenario Example](https://github.com/mehrandvd/skunit/assets/5070766/53d009a9-4a0b-44dc-91e0-b0be81b4c5a7)

### 8. Live Test Results

Watch your tests run in real-time with beautiful, readable output:

![Live Test Results](https://github.com/mehrandvd/skunit/assets/5070766/f3ef8a37-ceab-444f-b6f4-098557b61bfa)

## Installation & Setup

### 1. Install the Package

```bash
dotnet add package skUnit
```

### 2. Basic Setup

```csharp
public class MyChatTests
{
    private readonly ChatScenarioRunner _scenarioRunner;
    private readonly IChatClient _chatClient;

    public MyChatTests(ITestOutputHelper output)
    {
        // Configure your AI client (Azure OpenAI, OpenAI, etc.)
        _chatClient = new AzureOpenAIClient(endpoint, credential)
            .GetChatClient(deploymentName)
            .AsIChatClient();
            
        _scenarioRunner = new ChatScenarioRunner(_chatClient, output.WriteLine);
    }

    [Fact]
    public async Task TestChat()
    {
        var markdown = File.ReadAllText("scenario.md");
        var scenarios = ChatScenario.Parse(markdown);
        
        await _scenarioRunner.RunAsync(scenarios, _chatClient);
    }
}
```

### 3. Configuration

Set up your AI provider credentials:

```json
{
  "AzureOpenAI_ApiKey": "your-api-key",
  "AzureOpenAI_Endpoint": "https://your-endpoint.openai.azure.com/",
  "AzureOpenAI_Deployment": "your-deployment-name"
}
```

## Testing Multiple MCP Servers

Test complex scenarios involving multiple MCP servers working together:

```csharp
// Combine multiple MCP servers
var timeServer = await McpClientFactory.CreateAsync(timeTransport);
var weatherServer = await McpClientFactory.CreateAsync(weatherTransport);

var allTools = new List<AITool>();
allTools.AddRange(await timeServer.ListToolsAsync());
allTools.AddRange(await weatherServer.ListToolsAsync());

var chatClient = new ChatClientBuilder(baseChatClient)
    .ConfigureOptions(options => options.Tools = allTools.ToArray())
    .UseFunctionInvocation()
    .Build();
```

## Works with Any Test Framework

skUnit is completely test-framework agnostic! Here's the same test with different frameworks:

### xUnit
```csharp
public class GreetingTests
{
    private readonly ChatScenarioRunner ScenarioRunner;
    private readonly IChatClient systemUnderTestClient;

    public GreetingTests()
    {
        var assertionClient = /* assertion/evaluation model */;
        systemUnderTestClient = /* system under test model */;
        ScenarioRunner = new ChatScenarioRunner(assertionClient);
    }

    [Fact]
    public async Task TestGreeting()
    {
        var markdown = File.ReadAllText("greeting.md");
        var scenarios = ChatScenario.Parse(markdown);

        await ScenarioRunner.RunAsync(scenarios, systemUnderTestClient);
    }
}
```

### MSTest
```csharp
public class GreetingTests : TestClass
{
    private readonly ChatScenarioRunner ScenarioRunner;
    private readonly IChatClient systemUnderTestClient;

    public GreetingTests()
    {
        var assertionClient = /* assertion/evaluation model */;
        systemUnderTestClient = /* system under test model */;
        ScenarioRunner = new ChatScenarioRunner(assertionClient, TestContext.WriteLine);
    }

    [TestMethod]
    public async Task TestGreeting()
    {
        var scenarios = await ChatScenario.ParseFromResourceAsync(
            "MyProject.Scenarios.greeting.md", 
            typeof(GreetingTests).Assembly);
            
        await ScenarioRunner.RunAsync(scenarios, systemUnderTestClient);
    }
}
```

### NUnit  
```csharp
public class GreetingTests
{
    private readonly ChatScenarioRunner ScenarioRunner;
    private readonly IChatClient systemUnderTestClient;

    public GreetingTests()
    {
        var assertionClient = /* assertion/evaluation model */;
        systemUnderTestClient = /* system under test model */;
        ScenarioRunner = new ChatScenarioRunner(assertionClient, TestContext.WriteLine);
    }

    [Test]
    public async Task TestGreeting()
    {
        var markdown = File.ReadAllText("greeting.md");
        var scenarios = ChatScenario.Parse(markdown);

        await ScenarioRunner.RunAsync(scenarios, systemUnderTestClient);
    }
}
```

The core difference is just the logging integration - use `TestContext.WriteLine` for MSTest, `ITestOutputHelper.WriteLine` for xUnit, or `TestContext.WriteLine` for NUnit. Both patterns show:

- **Assertion Client**: Created once in the constructor for semantic evaluations
- **System Under Test Client**: The client whose behavior you're testing, passed to `RunAsync`


## Documentation

- **[Chat Scenario Spec](docs/chat-scenario-spec.md)** - Complete guide to writing chat scenarios
- **[ASSERT Statement Spec](docs/check-statements-spec.md)** - All available assertion types
- **[Test Framework Integration](docs/test-framework-integration.md)** - How to use skUnit with xUnit, MSTest, NUnit, and more
- **[MCP Testing Guide](docs/mcp-testing-guide.md)** - How to test Model Context Protocol servers
- **[Multi-Modal Support](docs/multi-modal-support.md)** - Working with images and other media
- **[Scenario Run Options](docs/scenario-run-options.md)** - Mitigate hallucinations with multi-run success thresholds

## Requirements

- **.NET 8.0** or higher
- **AI Provider** (Azure OpenAI, OpenAI, Anthropic, etc.) for semantic assertions
- **Test Framework** (xUnit, NUnit, MSTest - your choice!)

## Contributing

We welcome contributions! Check out our [issues](https://github.com/mehrandvd/skunit/issues) or submit a PR.

## Examples

Check out the `/demos` folder for complete examples:
- **Demo.TddRepl** - Interactive chat application testing
- **Demo.TddMcp** - MCP server integration testing  
- **Demo.TddShop** - Complex e-commerce chat scenarios
