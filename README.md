# skUnit
[![Build and Deploy](https://github.com/mehrandvd/skUnit/actions/workflows/build.yml/badge.svg)](https://github.com/mehrandvd/skUnit/actions/workflows/build.yml)
[![NuGet version (skUnit)](https://img.shields.io/nuget/v/skUnit.svg?style=flat)](https://www.nuget.org/packages/skUnit/)
[![NuGet downloads](https://img.shields.io/nuget/dt/skUnit.svg?style=flat)](https://www.nuget.org/packages/skUnit)

**skUnit** is a semantic testing framework for .NET that makes it easy to test AI-powered applications using simple, readable Markdown scenarios.

Test anything that talks to AI:
- 🤖 **IChatClient** implementations (Azure OpenAI, OpenAI, Anthropic, etc.)
- 🧠 **SemanticKernel** applications and plugins  
- 🔧 **MCP (Model Context Protocol) servers**
- 🛠️ **Custom AI integrations**

Write your tests in **Markdown**, run them with **any test framework** (xUnit, NUnit, MSTest), and get **live, readable results**.

## ⚡ Quick Start

Here's a simple test scenario in Markdown:

```md
# SCENARIO Simple Greeting

## [USER]
Hello!

## [AGENT]
Hi there! How can I help you today?

### CHECK SemanticCondition
It's a friendly greeting response
```

And here's how to test it with just a few lines of C#:

```csharp
[Fact]
public async Task TestGreeting()
{
    var markdown = File.ReadAllText("greeting.md");
    var scenarios = ChatScenario.LoadFromText(markdown);
    
    await ScenarioAssert.PassAsync(scenarios, myChatClient);
}
```

That's it! ✨ skUnit handles the conversation, calls your AI, and verifies the response makes sense.

## 🎯 Key Features

### 1. Start Simple: Basic Chat Scenarios

Test single interactions with basic checks:

```md
# SCENARIO Weather Check

## [USER]
What's the weather like?

## [AGENT]
It's sunny and 72°F outside

### CHECK ContainsAny
sunny, weather, temperature

### CHECK SemanticCondition
It provides weather information
```

### 2. Level Up: JSON Validation

Test structured responses with powerful JSON assertions:

```md
# SCENARIO User Info

## [USER]
Give me user info as JSON

## [AGENT]
{"name": "John", "age": 30, "city": "New York"}

### CHECK JsonCheck
{
  "name": ["NotEmpty"],
  "age": ["GreaterThan", 0],
  "city": ["SemanticCondition", "It's a real city name"]
}
```

### 3. Advanced: Function Call Testing

Verify your AI calls the right functions with the right parameters:

```md
# SCENARIO Time Query

## [USER]
What time is it?

## [AGENT]
It's currently 2:30 PM

### CHECK FunctionCall
{
  "function_name": "get_current_time"
}

### CHECK SemanticCondition
It mentions a specific time
```

### 4. Multi-Turn Conversations

Test complex conversations with multiple exchanges:
```md
# SCENARIO Height Discussion

## [USER]
Is Eiffel tall?

## [AGENT]
Yes it is

### CHECK SemanticCondition
It agrees that the Eiffel Tower is tall or expresses a positive sentiment.

## [USER]
What about Everest Mountain?

## [AGENT]
Yes it is tall too

### CHECK SemanticCondition
It agrees that Everest mountain is tall or expresses a positive sentiment.
```

![skUnit Chat Scenario Structure](https://github.com/mehrandvd/skunit/assets/5070766/156b0831-e4f3-4e4b-b1b0-e2ec868efb5f)

Each scenario can contain multiple sub-scenarios (conversation turns), and each response can have multiple CHECK statements to verify different aspects of the AI's behavior.

### 5. Readable Markdown Scenarios

Your test scenarios are just **valid Markdown files** - easy to read, write, and review:

![Markdown Scenario Example](https://github.com/mehrandvd/skunit/assets/5070766/53d009a9-4a0b-44dc-91e0-b0be81b4c5a7)

### 6. Live Test Results

Watch your tests run in real-time with beautiful, readable output:

![Live Test Results](https://github.com/mehrandvd/skunit/assets/5070766/f3ef8a37-ceab-444f-b6f4-098557b61bfa)

### 7. MCP Server Testing

Test [Model Context Protocol](https://modelcontextprotocol.io/) servers to ensure your tools work correctly:

```md
# SCENARIO MCP Time Server

## [USER]
What time is it?

## [AGENT]
It's currently 2:30 PM PST

### CHECK FunctionCall
{
  "function_name": "current_time"
}

### CHECK SemanticCondition
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

await ScenarioAssert.PassAsync(scenarios, chatClient);
```

## 🚀 Installation & Setup

### 1. Install the Package

```bash
dotnet add package skUnit
```

### 2. Basic Setup

```csharp
public class MyChatTests
{
    private readonly ScenarioAssert _scenarioAssert;
    private readonly IChatClient _chatClient;

    public MyChatTests(ITestOutputHelper output)
    {
        // Configure your AI client (Azure OpenAI, OpenAI, etc.)
        _chatClient = new AzureOpenAIClient(endpoint, credential)
            .GetChatClient(deploymentName)
            .AsIChatClient();
            
        _scenarioAssert = new ScenarioAssert(_chatClient, output.WriteLine);
    }

    [Fact]
    public async Task TestChat()
    {
        var markdown = File.ReadAllText("scenario.md");
        var scenarios = ChatScenario.LoadFromText(markdown);
        
        await _scenarioAssert.PassAsync(scenarios, _chatClient);
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

## 🧪 Testing Multiple MCP Servers

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

## 📚 Documentation

- **[Chat Scenario Spec](docs/chat-scenario-spec.md)** - Complete guide to writing chat scenarios
- **[CHECK Statement Spec](docs/check-statements-spec.md)** - All available assertion types
- **[MCP Testing Guide](docs/mcp-testing-guide.md)** - How to test Model Context Protocol servers
- **[Multi-Modal Support](docs/multi-modal-support.md)** - Working with images and other media

## 📋 Requirements

- **.NET 8.0** or higher
- **AI Provider** (Azure OpenAI, OpenAI, Anthropic, etc.) for semantic assertions
- **Test Framework** (xUnit, NUnit, MSTest - your choice!)

## 🤝 Contributing

We welcome contributions! Check out our [issues](https://github.com/mehrandvd/skunit/issues) or submit a PR.

## ⭐ Examples

Check out the `/demos` folder for complete examples:
- **Demo.TddRepl** - Interactive chat application testing
- **Demo.TddMcp** - MCP server integration testing  
- **Demo.TddShop** - Complex e-commerce chat scenarios

---

Start testing your AI applications with confidence! 🎯
