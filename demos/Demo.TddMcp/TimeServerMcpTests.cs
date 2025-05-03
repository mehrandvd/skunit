using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using skUnit;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers;
using Xunit.Abstractions;

namespace Demo.TddMcp
{
    public class TimeServerMcpTests
    {
        ScenarioAssert ScenarioAssert { get; set; }
        IChatClient ChatClient { get; set; }
        IConfiguration Configuration { get; set; }
        public TimeServerMcpTests(ITestOutputHelper output)
        {
            Configuration = new ConfigurationBuilder()
                                .AddUserSecrets<TimeServerMcpTests>()
                                .Build();

            var apiKey = Configuration["AzureOpenAI_ApiKey"] ?? throw new Exception("No ApiKey is provided.");
            var endpoint = Configuration["AzureOpenAI_Endpoint"] ?? throw new Exception("No Endpoint is provided.");
            var deploymentName = Configuration["AzureOpenAI_Deployment"] ?? throw new Exception("No Deployment is provided.");

            ChatClient = new AzureOpenAIClient(
                new Uri(endpoint),
                new System.ClientModel.ApiKeyCredential(apiKey)
            ).GetChatClient(deploymentName).AsIChatClient();

            ScenarioAssert = new ScenarioAssert(ChatClient, output.WriteLine);
        }

        [Fact]
        public async Task Tools_MustWork()
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

            await using var mcp = await McpClientFactory.CreateAsync(clientTransport);

            var tools = await mcp.ListToolsAsync();

            var builder = new ChatClientBuilder(ChatClient)
                          .ConfigureOptions(options =>
                          {
                              options.Tools = tools.ToArray();
                          })
                          .UseFunctionInvocation();

            var chatClient = builder.Build();

            var scenarioText = await File.ReadAllTextAsync("TestScenario.md");
            var scenario = ChatScenario.LoadFromText(scenarioText);
            await ScenarioAssert.PassAsync(scenario, chatClient);
        }
    }
}
