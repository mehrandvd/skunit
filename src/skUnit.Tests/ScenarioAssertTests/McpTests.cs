using Microsoft.Extensions.AI;
using skUnit.Tests.Infrastructure;
using System.ComponentModel;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using Xunit.Abstractions;
using Microsoft.Extensions.Configuration;

namespace skUnit.Tests.ScenarioAssertTests
{
    public class McpTests(ITestOutputHelper output) : SemanticTestBase(output)
    {
        [Fact]
        public async Task TimeServerMcp_MustWork()
        {
            var smitheryKey = Configuration["Smithery_Key"] ?? throw new Exception("No Smithery Key is provided.");

            var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "Time MCP Server",
                Command = "cmd",
                Arguments = [
                    "/c",
                    "npx",
                    "-y",
                    "@smithery/cli@latest",
                    "run",
                    "@yokingma/time-mcp",
                    "--key",
                    smitheryKey
                ],
            });

            await using var client = await McpClientFactory.CreateAsync(clientTransport);

            var tools = await client.ListToolsAsync();

            var builder = new ChatClientBuilder(BaseChatClient)
                          .ConfigureOptions(options =>
                          {
                              options.Tools ??= [.. tools];
                          })
                          .UseFunctionInvocation();

            var chatClient = builder.Build();

            var scenarios = await LoadChatScenarioAsync("GetCurrentTimeMcp");
            await ScenarioAssert.PassAsync(scenarios, chatClient);
        }
    }


}
