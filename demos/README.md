# skUnit Demos

This directory contains complete working examples demonstrating different aspects of skUnit testing.

## 🎮 Demo.TddRepl
**Interactive Chat Application Testing**

Shows how to use skUnit for test-driven development of a chat application (REPL - Read-Eval-Print Loop).

- **Location**: `Demo.TddRepl/`
- **Features**: SemanticKernel integration, plugin testing, interactive chat flows
- **Key Learning**: How to test conversational AI applications with semantic assertions

## 🔧 Demo.TddMcp  
**MCP Server Testing**

Demonstrates testing Model Context Protocol (MCP) servers with skUnit.

- **Location**: `Demo.TddMcp/`
- **Features**: MCP client setup, function call testing, tool validation
- **Key Learning**: How to test external AI tools and services

## 🛒 Demo.TddShop
**E-commerce Chat Scenarios**

Complex multi-scenario testing for e-commerce chat applications.

- **Location**: `Demo.TddShop/`
- **Features**: Multi-turn conversations, complex business logic testing
- **Key Learning**: How to test sophisticated chat workflows with multiple scenarios

## Running the Demos

Each demo is a complete .NET project that you can run independently:

```bash
cd Demo.TddRepl
dotnet restore Demo.TddRepl.sln
dotnet build Demo.TddRepl.sln --no-restore
dotnet test Demo.TddRepl.sln --no-build
```

## Prerequisites

- .NET 8.0 or higher
- Azure OpenAI API access (configure in user secrets)
- For MCP demos: Additional service API keys as needed

## Configuration

Set up your API keys using user secrets:

```bash
dotnet user-secrets set "AzureOpenAI_ApiKey" "your-key" --project Demo.TddRepl
dotnet user-secrets set "AzureOpenAI_Endpoint" "https://your-endpoint.openai.azure.com/" --project Demo.TddRepl
dotnet user-secrets set "AzureOpenAI_Deployment" "your-deployment-name" --project Demo.TddRepl
```

Each demo provides practical, real-world examples of how to use skUnit effectively in different scenarios.
