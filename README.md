# skUnit
[![Build and Deploy](https://github.com/mehrandvd/skUnit/actions/workflows/build.yml/badge.svg)](https://github.com/mehrandvd/skUnit/actions/workflows/build.yml)
[![NuGet version (skUnit)](https://img.shields.io/nuget/v/skUnit.svg?style=flat)](https://www.nuget.org/packages/skUnit/)
[![NuGet downloads](https://img.shields.io/nuget/dt/skUnit.svg?style=flat)](https://www.nuget.org/packages/skUnit)

**skUnit** is a semantic testing framework for .NET that makes it easy to test AI-powered applications using simple, readable Markdown scenarios.

Test anything that talks to AI:
- **IChatClient** implementations (Azure OpenAI, OpenAI, Anthropic, etc.)
- **SemanticKernel** applications and plugins  
- **MCP (Model Context Protocol) servers**
- **Custom AI integrations**

Write your tests in **Markdown**, run them with **any test framework** (xUnit, NUnit, MSTest), and get **live, readable results**.

## Quick Start

Here's a simple test scenario in Markdown:

```md
# SCENARIO Mountain Chat

## [USER]
What is the tallest mountain?

## [ASSISTANT]
The tallest mountain is Everest! (OPTIONAL)

### ASSERT SemanticCondition
It mentions Mount Everest.
```

And here's how to test it with just a few lines of C#:

```csharp
[Fact]
public async Task SimpleTest()
{
    var markdown = File.ReadAllText("mountain-chat.md");
    var scenarios = ChatScenario.LoadFromText(markdown);

    await ScenarioRunner.RunAsync(scenarios, systemUnderTestClient);
}
```

Note that in this example, the **agent message** is just for clarity and is not being used and is optional. So the following test scenario is equivalent:

```md
## [USER]
What is the tallest mountain?

## [ASSISTANT]

### ASSERT SemanticCondition
It mentions Mount Everest.
```

That's it! skUnit handles the conversation, calls your AI, and verifies the response makes sense.

## Key Features

### 1. Basic Chat Scenarios

Test single interactions with basic checks:

```md
## [USER]
Is Everest a mountain or a Tree?

## [ASSISTANT]

### ASSERT ContainsAny
mountain

### ASSERT SemanticCondition
It mentions the mountain
```

### 2. JSON Validation

Test structured responses with powerful JSON assertions:

```md
# SCENARIO User Info

## [USER]
Give me the most expensive product info as a JSON like this:
{"id": 12, "title": "The product", "price": 0, "description": "the description of the product"}

## [ASSISTANT]
{"id": 12, "title": "Surface Studio 2", "price": 3000, "description: "It is a very high-quality laptop"}

### ASSERT JsonCheck
{
  "id": ["NotEmpty"],
  "title": ["Contains", "Surface"],
  "price": ["Equal", 3000],
  "description": ["SemanticCondition", "It mentions the quality of the laptop."]
}
```

### 3. Function Call Testing

Verify your AI calls the right functions (MCP maybe) with the right parameters:

```md
# SCENARIO Time Query

## [USER]
What time is it?

## [ASSISTANT]
It's currently 2:30 PM

### ASSERT FunctionCall
{
  "function_name": "get_current_time"
}
```

Even you can assert the called parameters:

### ASSERT FunctionCall
{
  "function_name": "GetFoodMenu",
  "arguments": {
    "mood": ["Equals", "Happy"]
  }
}


### 4. Multi-Turn Conversations

Test complex conversations with multiple exchanges:
```md
# SCENARIO Height Discussion

## [USER]
Is Eiffel tall?

## [ASSISTANT]
Yes it is

### ASSERT SemanticCondition
It agrees that the Eiffel Tower is tall or expresses a positive sentiment.

## [USER]
What about Everest?

## [ASSISTANT]
Yes it is tall too

### ASSERT SemanticCondition
It agrees that Everest is tall or expresses a positive sentiment.
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

var chatClient = new ChatClientBuilder(baseChatClient)
    .ConfigureOptions(options => options.Tools = tools.ToArray())
    .UseFunctionInvocation()
    .Build();

// In your test class constructor:
var assertionClient = /* assertion/evaluation model */;
ScenarioRunner = new ChatScenarioRunner(assertionClient);

// In your test:
await ScenarioRunner.RunAsync(scenarios, chatClient);
```

### 6. Mitigating Hallucinations with ScenarioRunOptions

LLM outputs can vary between runs. A single spurious response shouldn't fail your build if the model normally behaves correctly.

Use `ScenarioRunOptions` to execute each scenario multiple times and require only a percentage to pass. This adds statistical robustness without eliminating genuine regressions.

```csharp
var options = new ScenarioRunOptions
{
    TotalRuns = 3,        // Run the whole scenario three times
    MinSuccessRate = 0.67 // At least 2 of 3 runs must pass
};

// In your test class constructor:
var assertionClient = /* assertion/evaluation model */;
ScenarioRunner = new ChatScenarioRunner(assertionClient);

// In your test:
await ScenarioRunner.RunAsync(scenarios, systemUnderTestClient, options: options);
```

Recommended starting points:
- Deterministic / low-temp prompts: `TotalRuns = 1`, `MinSuccessRate = 1.0`
- Function / tool invocation: `TotalRuns = 3`, `MinSuccessRate = 0.67`
- Creative generation: `TotalRuns = 5`, `MinSuccessRate = 0.6`
- Critical CI gating: `TotalRuns = 5`, `MinSuccessRate = 0.8`

Failure message example:
```
Only 40% of rounds passed, which is below the required success rate of 80%
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
        var scenarios = ChatScenario.LoadFromText(markdown);
        
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
        var scenarios = ChatScenario.LoadFromText(markdown);

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
        var scenarios = await ChatScenario.LoadFromResourceAsync(
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
        var scenarios = ChatScenario.LoadFromText(markdown);

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
