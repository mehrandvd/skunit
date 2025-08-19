# skUnit - Semantic Testing Framework for IChatClient and SemanticKernel

skUnit is a .NET testing framework for creating semantic tests for IChatClient implementations and SemanticKernel units using markdown scenario files.

**ALWAYS** reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Bootstrap, Build, and Test
- **Initial setup**: `dotnet restore src/skUnit.sln` -- takes 16 seconds. NEVER CANCEL. Set timeout to 30+ seconds.
- **Build the solution**: `dotnet build src/skUnit.sln --no-restore` -- takes 12 seconds. NEVER CANCEL. Set timeout to 30+ seconds.
- **Run unit tests (works offline)**: `dotnet test src/skUnit.sln --no-build --filter "FullyQualifiedName~JsonCheckAssertionTests|FullyQualifiedName~FunctionCallAssertionTests|FullyQualifiedName~ParseChatScenarioTests"` -- takes 1 second
- **Full test suite (requires API keys)**: `dotnet test src/skUnit.sln --no-build` -- requires Azure OpenAI configuration

### Code Quality
- **Format code**: `dotnet format src/skUnit.sln` -- takes 8 seconds. NEVER CANCEL. Set timeout to 30+ seconds.
- **Verify formatting**: `dotnet format src/skUnit.sln --verify-no-changes` -- fails if formatting needed
- **ALWAYS** run `dotnet format src/skUnit.sln` before committing changes or CI will have formatting issues

### Demo Projects
- **TDD REPL Demo**: Located in `demos/Demo.TddRepl/`
  - Build: `cd demos/Demo.TddRepl && dotnet restore Demo.TddRepl.sln && dotnet build Demo.TddRepl.sln --no-restore`
  - Shows how to use skUnit for test-driven development of chat applications
- **MCP Demo**: Located in `demos/Demo.TddMcp/`
  - Shows integration with Model Context Protocol tools
- **Shop Demo**: Located in `demos/Demo.TddShop/`
  - Complex multi-scenario chat testing

## Requirements and Configuration

### System Requirements
- **.NET 8.0** (verified working with 8.0.118)
- **Azure OpenAI API access** (for integration tests and semantic validation)

### Required Environment Variables for Full Testing
Set these in your environment or user secrets for integration tests:
```bash
AzureOpenAI_ApiKey=your-api-key
AzureOpenAI_Endpoint=https://your-endpoint.openai.azure.com/
AzureOpenAI_Deployment=your-deployment-name
Smithery_Key=your-smithery-key  # For MCP tests only
```

### Configuration Files
- `src/skUnit.Tests/skUnit.Tests.csproj` has `<UserSecretsId>8f5163d1-e8e8-4a8e-9186-b473280a19b4</UserSecretsId>`
- Use `dotnet user-secrets set "AzureOpenAI_ApiKey" "your-key" --project src/skUnit.Tests` to configure secrets

## Validation and Testing

### ALWAYS run these validation steps after making changes:
1. **Format code**: `dotnet format src/skUnit.sln`
2. **Build**: `dotnet build src/skUnit.sln --no-restore`
3. **Unit tests**: `dotnet test src/skUnit.sln --no-build --filter "FullyQualifiedName~JsonCheckAssertionTests|FullyQualifiedName~FunctionCallAssertionTests|FullyQualifiedName~ParseChatScenarioTests"`
4. **If you have API keys configured**: `dotnet test src/skUnit.sln --no-build`

### Manual Validation Scenarios
After making changes to scenario parsing or assertion logic:
1. **Test scenario parsing**: Create a test .md file and verify it parses correctly
2. **Test specific assertion types**: Run individual assertion tests for the type you modified
3. **Integration test**: If you have API keys, run a complete chat scenario test

Example validation scenario for chat testing:
```markdown
# SCENARIO Simple Test

## [USER]
Hello

## [AGENT]
Hi there!

### CHECK SemanticCondition
It's a greeting response
```

## Key Project Structure

### Main Source Code (`src/skUnit/`)
- **`Asserts/`**: Core assertion logic including `ScenarioAssert` class
- **`Scenarios/`**: Scenario parsing and execution logic
  - **`Parsers/`**: Markdown parsing for scenarios and assertions
  - **`Parsers/Assertions/`**: Individual assertion type implementations
- **`Exceptions/`**: Custom exception types for semantic testing

### Test Projects (`src/skUnit.Tests/`)
- **`AssertionTests/`**: Unit tests for individual assertion types (work offline)
- **`ScenarioAssertTests/`**: Integration tests requiring API keys
- **`ScenarioParseTests/`**: Tests for markdown parsing logic (work offline)
- **`Infrastructure/`**: Base test classes and configuration
- **`Samples/`**: Embedded test scenario .md files

### Frequently Used Commands Output

#### Repository Root Structure
```
.github/          # GitHub workflows and configuration
.gitignore
LICENSE
README.md
demos/           # Example projects showing skUnit usage
docs/            # Documentation for scenarios and CHECK statements
src/             # Main source code and tests
```

#### Main Solution Structure
```
src/
├── skUnit.sln                    # Main solution file
├── skUnit/                       # Core library project
│   ├── skUnit.csproj            # .NET 8.0, NuGet package config
│   ├── Asserts/                 # Core assertion and testing logic
│   ├── Scenarios/               # Scenario parsing and execution
│   └── Exceptions/              # Custom exceptions
└── skUnit.Tests/                # Test project
    ├── skUnit.Tests.csproj      # Test project with xUnit
    ├── AssertionTests/          # Unit tests (work offline)
    ├── ScenarioAssertTests/     # Integration tests (need API keys)
    └── Infrastructure/          # Test infrastructure
```

## Common Tasks and Workflows

### Adding New Assertion Types
1. Create new assertion class in `src/skUnit/Scenarios/Parsers/Assertions/`
2. Implement `IKernelAssertion` interface
3. Add parsing logic to `KernelAssertionParser.cs`
4. Create unit tests in `src/skUnit.Tests/AssertionTests/`
5. Add integration tests in scenario .md files

### Working with Scenario Files
- **Location**: Test scenarios in `src/skUnit.Tests/ScenarioAssertTests/Samples/`
- **Format**: Valid Markdown with special headers like `## [USER]`, `## [AGENT]`, `### CHECK`
- **Embedded Resources**: Test scenarios are embedded in the test assembly

### Working with CHECK Statements
Available assertion types:
- `CHECK Equals` - Exact string match
- `CHECK ContainsAll` - Contains all specified words
- `CHECK ContainsAny` - Contains any of specified words
- `CHECK SemanticCondition` - AI-powered semantic validation
- `CHECK SemanticSimilar` - AI-powered similarity check
- `CHECK JsonCheck` - JSON structure and content validation
- `CHECK FunctionCall` - Validate function calls in chat responses

### Integration with CI/CD
- **GitHub Actions**: `.github/workflows/build.yml` runs build only
- **Test workflow**: `.github/workflows/test.yml` runs with secrets (manual trigger)
- **Formatting**: Always run `dotnet format` before commit to avoid CI failures

## Timing Expectations and Timeouts

### Build Commands (NEVER CANCEL)
- `dotnet restore`: 16 seconds - Set timeout to 30+ seconds
- `dotnet build`: 12 seconds - Set timeout to 30+ seconds
- `dotnet format`: 8 seconds - Set timeout to 30+ seconds

### Test Commands
- Unit tests (offline): 1 second - Set timeout to 10+ seconds
- Full test suite (with API): Variable (depends on OpenAI response time) - Set timeout to 120+ seconds
- Individual integration test: 5-30 seconds - Set timeout to 60+ seconds

### Demo Project Build
- Demo restore + build: 12-14 seconds total - Set timeout to 30+ seconds

## Troubleshooting Common Issues

### Build Warnings
The project has some nullable reference warnings that are expected and don't affect functionality:
- CS8604, CS8602, CS8625 warnings in assertion and test code
- These warnings should not block builds or prevent functionality

### Test Failures
- **"No ApiKey is provided"**: Configure Azure OpenAI credentials for integration tests
- **Formatting errors**: Run `dotnet format src/skUnit.sln` to fix
- **Parse errors in scenarios**: Check markdown format matches expected headers and CHECK statement syntax

### Demo Project Issues
- Demos require same Azure OpenAI configuration as main test project
- Some demos may require additional secrets (like Smithery_Key for MCP demo)

## Important Notes for Development

- **Target Framework**: .NET 8.0 (net8.0) - do not change without careful consideration
- **Package Dependencies**: Core dependencies are Microsoft.Extensions.AI, SemanticKernel.Abstractions, Markdig
- **Test Framework**: xUnit with test output helpers for scenario execution logging
- **NuGet Package**: This project publishes to NuGet as "skUnit" package
- **Markdown Processing**: Uses Markdig for parsing scenario files
- **Semantic Validation**: Powered by SemanticValidation package for AI-based assertions