using Microsoft.Extensions.AI;
using skUnit.Tests.Infrastructure;
using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using skUnit;
using Xunit.Abstractions;
using skUnit.Exceptions;

namespace skUnit.Tests.ScenarioAssertTests
{
    public class McpTests(ITestOutputHelper output) : SemanticTestBase(output)
    {
        private const string TimeServerPath = "";
        private const string EverythingServerPath = "";

        [Fact]
        public async Task McpUrl_DirectValidation_MustWork()
        {
            // Arrange
            var assert = new SemanticAssert(BaseChatClient);
            assert.UseMcpEndpoint("http://localhost:5000");

            // Act & Assert
            await assert.ValidateMcpResponseAsync(
                "The time is 10:23",
                """
                {
                    "semantic": "It mentions a time.",
                    "function_call": {
                        "name": "current_time"
                    }
                }
                """
            );
        }

        [Fact]
        public async Task McpProcess_WithConfiguration_MustWork()
        {
            // Arrange
            var assert = new SemanticAssert(BaseChatClient);
            assert.UseMcpEndpoint("http://localhost:5000");

            var processConfig = $$"""
                                  {
                                      "timemcp": {
                                          "command": "dotnet",
                                          "args": [
                                              "{{TimeServerPath}}"
                                          ]
                                      },
                                      "everything": {
                                          "command": "dotnet",
                                          "args": [
                                              "{{EverythingServerPath}}"
                                          ]
                                      }
                                  }
                                  """;

            assert.ConfigureMcpProcesses(processConfig);

            // Act & Assert
            await assert.ValidateMcpResponseWithProcessAsync(
                "The time is 10:23",
                """
                {
                    "semantic": "It mentions a time.",
                    "function_call": {
                        "name": "current_time"
                    }
                }
                """,
                "timemcp"
            );
        }

        [Fact]
        public async Task McpProcess_WithoutConfiguration_ShouldThrow()
        {
            // Arrange
            var assert = new SemanticAssert(BaseChatClient);
            assert.UseMcpEndpoint("http://localhost:5000");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                assert.ValidateMcpResponseWithProcessAsync(
                    "The time is 10:23",
                    """
                    {
                        "semantic": "It mentions a time."
                    }
                    """,
                    "timemcp"
                )
            );
        }

        [Fact]
        public async Task McpProcess_InvalidProcessName_ShouldThrow()
        {
            // Arrange
            var processConfig = """
                                {
                                    "timemcp": {
                                        "command": "dotnet",
                                        "args": [
                                            "/path/to/time/server.dll"
                                        ]
                                    }
                                }
                                """;

            var assert = new SemanticAssert("http://localhost:5000", processConfig);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                assert.ValidateMcpResponseWithProcessAsync(
                    "The time is 10:23",
                    """
                    {
                        "semantic": "It mentions a time."
                    }
                    """,
                    "nonexistent"
                )
            );
        }

        [Fact]
        public async Task TimeServerMcp_MustWork()
        {
            var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "Time MCP Server",
                Command = "cmd",
                Arguments =
                [
                    "/c",
                    "npx",
                    "-y",
                    "@smithery/cli@latest",
                    "run",
                    "@yokingma/time-mcp"
                ],
            });

            await using var client = await McpClientFactory.CreateAsync(clientTransport);

            var tools = await client.ListToolsAsync();

            var builder = new ChatClientBuilder(BaseChatClient)
                .ConfigureOptions(options => { options.Tools ??= [.. tools]; })
                .UseFunctionInvocation();

            var chatClient = builder.Build();

            var scenarios = await LoadChatScenarioAsync("GetCurrentTimeMcp");
            await ScenarioAssert.PassAsync(scenarios, chatClient);
        }

        [Fact]
        public async Task McpValidation_InvalidJson_ShouldThrow()
        {
            // Arrange
            var assert = new SemanticAssert(BaseChatClient);
            assert.UseMcpEndpoint("http://localhost:5000");

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() =>
                assert.ValidateMcpResponseAsync(
                    "The time is 10:23",
                    "invalid json"
                )
            );
        }

        [Fact]
        public async Task McpValidation_EmptyResponse_ShouldThrow()
        {
            // Arrange
            var assert = new SemanticAssert(BaseChatClient);
            assert.UseMcpEndpoint("http://localhost:5000");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                assert.ValidateMcpResponseAsync(
                    "",
                    """
                    {
                        "semantic": "It mentions a time."
                    }
                    """
                )
            );
        }
    }
}