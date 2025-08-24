# MCP Server Testing Guide

This guide shows you how to test [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) servers using skUnit.

## What is MCP?

Model Context Protocol (MCP) is an open protocol that enables AI applications to securely connect to data sources and tools. MCP servers provide tools that AI models can call to access information or perform actions.

## Testing MCP Servers with skUnit

skUnit makes it easy to test that your MCP servers work correctly with AI models. You can verify that:
- The right tools are called
- Tools are called with correct parameters  
- Responses are handled properly
- Multiple MCP servers work together

## Basic MCP Testing Setup

```csharp
[Fact]
public async Task TestMcpServer()
{
    // 1. Setup MCP client transport (stdio, HTTP, etc.)
    var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
    {
        Name = "My MCP Server",
        Command = "node",
        Arguments = ["my-mcp-server.js"]
    });

    // 2. Create MCP client and get tools
    await using var mcp = await McpClientFactory.CreateAsync(clientTransport);
    var tools = await mcp.ListToolsAsync();

    // 3. Setup chat client with MCP tools
    var chatClient = new ChatClientBuilder(baseChatClient)
        .ConfigureOptions(options => options.Tools = tools.ToArray())
        .UseFunctionInvocation()
        .Build();

    // 4. Run your test scenario
    var markdown = File.ReadAllText("mcp-test.md");
    var scenarios = await ChatScenario.LoadFromText(markdown);
    
    await ScenarioAssert.PassAsync(scenarios, chatClient);
}
```

## Example MCP Test Scenarios

### Testing Time Server

```md
# SCENARIO Time Server Test

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

### Testing Calendar Server

```md
# SCENARIO Calendar Operations

## [USER]
Schedule a meeting for tomorrow at 2 PM

## [AGENT]
I've scheduled your meeting for tomorrow at 2:00 PM

### CHECK FunctionCall
{
  "function_name": "schedule_event",
  "arguments": {
    "title": ["NotEmpty"],
    "datetime": ["SemanticCondition", "It represents tomorrow at 2 PM"]
  }
}

### CHECK SemanticCondition
It confirms the meeting was scheduled
```

### Testing File System Server

```md
# SCENARIO File Operations

## [USER]
List files in the current directory

## [AGENT]
Here are the files: file1.txt, file2.md, folder1/

### CHECK FunctionCall
{
  "function_name": "list_directory"
}

### CHECK SemanticCondition
It lists file names or mentions files
```

## Testing Multiple MCP Servers

You can test scenarios involving multiple MCP servers working together:

```csharp
[Fact]
public async Task TestMultipleMcpServers()
{
    // Setup multiple MCP servers
    var timeServer = await McpClientFactory.CreateAsync(timeTransport);
    var weatherServer = await McpClientFactory.CreateAsync(weatherTransport);
    var calendarServer = await McpClientFactory.CreateAsync(calendarTransport);

    // Combine all tools
    var allTools = new List<AITool>();
    allTools.AddRange(await timeServer.ListToolsAsync());
    allTools.AddRange(await weatherServer.ListToolsAsync());
    allTools.AddRange(await calendarServer.ListToolsAsync());

    // Setup chat client with all tools
    var chatClient = new ChatClientBuilder(baseChatClient)
        .ConfigureOptions(options => options.Tools = allTools.ToArray())
        .UseFunctionInvocation()
        .Build();

    // Test complex scenario
    var scenarios = await ChatScenario.LoadFromText(complexScenario);
    await ScenarioAssert.PassAsync(scenarios, chatClient);
}
```

### Multi-Server Test Scenario

```md
# SCENARIO Multi-Server Coordination

## [USER]
Check the weather and schedule a meeting for tomorrow if it's sunny

## [AGENT]
I checked the weather - it's going to be sunny tomorrow! I've scheduled your meeting for 2 PM.

### CHECK FunctionCall
{
  "function_name": "get_weather"
}

### CHECK FunctionCall
{
  "function_name": "schedule_event",
  "arguments": {
    "datetime": ["SemanticCondition", "It's for tomorrow"]
  }
}

### CHECK SemanticCondition
It mentions both weather information and confirms meeting scheduling
```

## MCP Testing Best Practices

1. **Isolate Server Testing**: Test each MCP server individually before testing combinations
2. **Verify Tool Discovery**: Ensure `ListToolsAsync()` returns expected tools
3. **Test Error Scenarios**: Verify how your system handles MCP server failures
4. **Check Parameters**: Use argument assertions to verify tools are called with correct data
5. **Mock External Dependencies**: Use test doubles for external services your MCP server depends on

## Configuration for MCP Testing

Some MCP servers require configuration (API keys, endpoints, etc.). Use environment variables or user secrets:

```json
{
  "MCP_TimeServer_Endpoint": "https://time-api.example.com",
  "MCP_Weather_ApiKey": "your-weather-api-key"
}
```

## Troubleshooting MCP Tests

- **Tool not found**: Check that the MCP server is running and tools are properly registered
- **Function not called**: Verify the AI model has access to the tool and understands when to use it
- **Parameter mismatch**: Ensure your CHECK FunctionCall assertions match the actual tool schema
- **Transport issues**: Check MCP server logs and transport configuration

For more details on CHECK statements, see [CHECK Statement Spec](check-statements-spec.md).