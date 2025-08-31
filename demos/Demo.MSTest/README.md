# Demo.MSTest - MSTest Integration Example

This demo shows how to use skUnit with MSTest framework for semantic testing of chat applications.

## Key Features Demonstrated

- **MSTest Attributes**: Using `[TestClass]`, `[TestMethod]`, `[DataTestMethod]`, and `[ClassInitialize]`
- **TestContext Integration**: Proper logging integration using `TestContext.WriteLine`
- **Data-Driven Tests**: Using `[DataRow]` for parameterized tests
- **Scenario Loading**: Loading test scenarios from embedded markdown files
- **Function-Style Testing**: Alternative approach using `getAnswerFunc` delegate

## MSTest vs xUnit Key Differences

### Test Class Setup
```csharp
// MSTest
[TestClass]
public class ChatScenarioTests
{
    public TestContext TestContext { get; set; } = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context) { }

    [TestMethod]
    public async Task MyTest() { }
}

// xUnit equivalent
public class ChatScenarioTests
{
    public ChatScenarioTests(ITestOutputHelper output) { }

    [Fact]
    public async Task MyTest() { }
}
```

### Logging Integration
```csharp
// MSTest
var scenarioAssert = new ScenarioAssert(_chatClient, TestContext.WriteLine);

// xUnit
var scenarioAssert = new ScenarioAssert(_chatClient, output.WriteLine);
```

### Data-Driven Tests
```csharp
// MSTest
[DataTestMethod]
[DataRow("Scenario1")]
[DataRow("Scenario2")]
public async Task TestScenarios(string scenarioName) { }

// xUnit
[Theory]
[InlineData("Scenario1")]
[InlineData("Scenario2")]
public async Task TestScenarios(string scenarioName) { }
```

## Running the Demo

1. **Configure Azure OpenAI secrets**:
```bash
cd Demo.MSTest
dotnet user-secrets set "AzureOpenAI_ApiKey" "your-key"
dotnet user-secrets set "AzureOpenAI_Endpoint" "https://your-endpoint.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI_Deployment" "your-deployment-name"
```

2. **Build and test**:
```bash
dotnet restore Demo.MSTest.sln
dotnet build Demo.MSTest.sln --no-restore
dotnet test Demo.MSTest.sln --no-build
```

## Scenarios Included

- **SimpleGreeting.md**: Basic semantic condition testing
- **GetCurrentTimeChat.md**: Multi-turn conversation validation
- **JsonUserInfo.md**: JSON structure and content validation

This demo proves that skUnit works seamlessly with MSTest - the core library is completely test-framework agnostic!