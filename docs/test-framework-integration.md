# Test Framework Integration Guide

skUnit is designed to be completely test-framework agnostic. This guide shows how to integrate skUnit with different test frameworks.

## Core Principles

skUnit's core library (`skUnit` NuGet package) has no dependencies on any specific test framework. It only depends on:
- `Microsoft.Extensions.AI` - For chat client abstractions
- `Microsoft.SemanticKernel.Abstractions` - For kernel abstractions
- `Markdig` - For markdown parsing
- `SemanticValidation` - For semantic assertions

This means you can use skUnit with any .NET testing framework.

## Framework-Specific Patterns

### xUnit

xUnit uses `ITestOutputHelper` for test output logging.

#### Basic Setup
```csharp
using Xunit;
using Xunit.Abstractions;
using skUnit;
using skUnit.Scenarios;

public class ChatTests
{
    private readonly ITestOutputHelper _output;
    private readonly IChatClient _chatClient;

    public ChatTests(ITestOutputHelper output)
    {
        _output = output;
        _chatClient = CreateChatClient(); // Your setup
    }

    [Fact]
    public async Task SimpleTest()
    {
        var scenarioAssert = new ScenarioAssert(_chatClient, _output.WriteLine);
        var scenarios = await ChatScenario.LoadFromResourceAsync(
            "MyProject.Scenarios.test.md", 
            typeof(ChatTests).Assembly);
        
        await scenarioAssert.PassAsync(scenarios);
    }
}
```

#### Data-Driven Tests
```csharp
[Theory]
[InlineData("Scenario1")]
[InlineData("Scenario2")]
public async Task TestScenarios(string scenarioName)
{
    var scenarioAssert = new ScenarioAssert(_chatClient, _output.WriteLine);
    var scenarios = await ChatScenario.LoadFromResourceAsync(
        $"MyProject.Scenarios.{scenarioName}.md",
        typeof(ChatTests).Assembly);
    
    await scenarioAssert.PassAsync(scenarios);
}
```

### MSTest

MSTest uses `TestContext` for test output logging and supports class-level initialization.

#### Basic Setup
```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using skUnit;
using skUnit.Scenarios;

[TestClass]
public class ChatTests
{
    private static IChatClient _chatClient = null!;
    public TestContext TestContext { get; set; } = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _chatClient = CreateChatClient(); // Your setup
    }

    [TestMethod]
    public async Task SimpleTest()
    {
        var scenarioAssert = new ScenarioAssert(_chatClient, TestContext.WriteLine);
        var scenarios = await ChatScenario.LoadFromResourceAsync(
            "MyProject.Scenarios.test.md", 
            typeof(ChatTests).Assembly);
        
        await scenarioAssert.PassAsync(scenarios);
    }
}
```

#### Data-Driven Tests
```csharp
[DataTestMethod]
[DataRow("Scenario1")]
[DataRow("Scenario2")]
public async Task TestScenarios(string scenarioName)
{
    var scenarioAssert = new ScenarioAssert(_chatClient, TestContext.WriteLine);
    var scenarios = await ChatScenario.LoadFromResourceAsync(
        $"MyProject.Scenarios.{scenarioName}.md",
        typeof(ChatTests).Assembly);
    
    await scenarioAssert.PassAsync(scenarios);
}
```

### NUnit

NUnit uses `TestContext` similar to MSTest but with slightly different patterns.

#### Basic Setup
```csharp
using NUnit.Framework;
using skUnit;
using skUnit.Scenarios;

[TestFixture]
public class ChatTests
{
    private IChatClient _chatClient = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _chatClient = CreateChatClient(); // Your setup
    }

    [Test]
    public async Task SimpleTest()
    {
        var scenarioAssert = new ScenarioAssert(_chatClient, TestContext.WriteLine);
        var scenarios = await ChatScenario.LoadFromResourceAsync(
            "MyProject.Scenarios.test.md", 
            typeof(ChatTests).Assembly);
        
        await scenarioAssert.PassAsync(scenarios);
    }
}
```

#### Data-Driven Tests
```csharp
[TestCase("Scenario1")]
[TestCase("Scenario2")]
public async Task TestScenarios(string scenarioName)
{
    var scenarioAssert = new ScenarioAssert(_chatClient, TestContext.WriteLine);
    var scenarios = await ChatScenario.LoadFromResourceAsync(
        $"MyProject.Scenarios.{scenarioName}.md",
        typeof(ChatTests).Assembly);
    
    await scenarioAssert.PassAsync(scenarios);
}
```

## Logging Integration

The key difference between test frameworks is the logging mechanism:

| Framework | Logging Method | Pattern |
|-----------|----------------|---------|
| xUnit | `ITestOutputHelper.WriteLine` | `new ScenarioAssert(chatClient, output.WriteLine)` |
| MSTest | `TestContext.WriteLine` | `new ScenarioAssert(chatClient, TestContext.WriteLine)` |
| NUnit | `TestContext.WriteLine` | `new ScenarioAssert(chatClient, TestContext.WriteLine)` |

For modern logging integration, you can also use `ILogger<ScenarioAssert>`:

```csharp
// Modern approach with ILogger
var logger = loggerFactory.CreateLogger<ScenarioAssert>();
var scenarioAssert = new ScenarioAssert(chatClient, logger);
```

## Project Configuration

### Package References

All frameworks need these core packages:

```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
<PackageReference Include="Microsoft.Extensions.AI" Version="9.7.1" />
<PackageReference Include="Azure.AI.OpenAI" Version="2.1.0" />
<!-- Add your preferred test framework -->
```

#### xUnit
```xml
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.3" />
```

#### MSTest
```xml
<PackageReference Include="MSTest.TestAdapter" Version="3.1.1" />
<PackageReference Include="MSTest.TestFramework" Version="3.1.1" />
```

#### NUnit
```xml
<PackageReference Include="NUnit" Version="4.0.1" />
<PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
```

### Embedded Resources

For all frameworks, embed your scenario files:

```xml
<ItemGroup>
  <EmbeddedResource Include="Scenarios\*.md">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </EmbeddedResource>
</ItemGroup>
```

## Advanced Patterns

### Shared Base Class (xUnit)
```csharp
public abstract class SemanticTestBase : IDisposable
{
    protected readonly ITestOutputHelper Output;
    protected readonly IChatClient ChatClient;
    protected readonly ScenarioAssert ScenarioAssert;

    protected SemanticTestBase(ITestOutputHelper output)
    {
        Output = output;
        ChatClient = CreateChatClient();
        ScenarioAssert = new ScenarioAssert(ChatClient, output.WriteLine);
    }

    protected virtual IChatClient CreateChatClient() => /* your implementation */;
    
    public virtual void Dispose() => /* cleanup */;
}

public class MyTests : SemanticTestBase
{
    public MyTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task MyTest()
    {
        var scenarios = await ChatScenario.LoadFromResourceAsync(/* ... */);
        await ScenarioAssert.PassAsync(scenarios);
    }
}
```

### Shared Base Class (MSTest)
```csharp
[TestClass]
public abstract class SemanticTestBase
{
    protected static IChatClient ChatClient = null!;
    public TestContext TestContext { get; set; } = null!;

    [ClassInitialize]
    public static void BaseClassInitialize(TestContext context)
    {
        ChatClient = CreateChatClient();
    }

    protected static virtual IChatClient CreateChatClient() => /* your implementation */;

    protected ScenarioAssert CreateScenarioAssert()
    {
        return new ScenarioAssert(ChatClient, TestContext.WriteLine);
    }
}

[TestClass]
public class MyTests : SemanticTestBase
{
    [TestMethod]
    public async Task MyTest()
    {
        var scenarioAssert = CreateScenarioAssert();
        var scenarios = await ChatScenario.LoadFromResourceAsync(/* ... */);
        await scenarioAssert.PassAsync(scenarios);
    }
}
```

## Examples

See the demo projects for complete working examples:
- `demos/Demo.TddRepl/` - xUnit example
- `demos/Demo.MSTest/` - MSTest example
- `demos/Demo.TddMcp/` - xUnit with MCP integration

## Troubleshooting

### Common Issues

1. **Scenario file not found**: Ensure files are marked as `EmbeddedResource`
2. **No logging output**: Check that you're passing the correct logging delegate
3. **API key not found**: Configure user secrets or environment variables

### Framework-Specific Issues

#### MSTest
- Ensure `TestContext` property is properly initialized
- Use `[ClassInitialize]` for shared setup, not constructor
- Remember `[DataTestMethod]` requires `[DataRow]` attributes

#### xUnit
- Constructor is called for each test, not shared
- Use `IClassFixture<T>` for shared setup across tests
- `[Theory]` requires `[InlineData]` or other data attributes

#### NUnit
- Use `[OneTimeSetUp]` for shared setup
- `[TestCase]` is similar to xUnit's `[InlineData]`
- TestContext works similarly to MSTest

This guide proves that skUnit truly works with any test framework - choose the one your team prefers!