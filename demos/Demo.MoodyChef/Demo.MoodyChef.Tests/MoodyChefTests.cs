using Azure;
using Azure.AI.OpenAI;
using Demo.MoodyChef.Console;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using OpenAI.Chat;
using skUnit;
using skUnit.Scenarios;
using Xunit.Sdk;
using Xunit.v3;

namespace Demo.MoodyChef.Tests
{
    
    public class MoodyChefTests
    {
        IChatClient chatClient;

        public MoodyChefTests(ITestOutputHelper output)
        {
            var builder = new ConfigurationBuilder()
                          .AddUserSecrets<MoodyChefTests>()
                          .AddEnvironmentVariables();

            var configuration = builder.Build();

            var apiKey = configuration["AzureOpenAI_ApiKey"] ??
                         throw new Exception("No ApiKey is provided.");
            var endpoint = configuration["AzureOpenAI_Endpoint"] ??
                           throw new Exception("No Endpoint is provided.");
            var deploymentName = configuration["AzureOpenAI_Deployment"] ??
                                 throw new Exception("No Deployment is provided.");


            chatClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey))
                         .GetChatClient(deploymentName)
                         .AsIChatClient();

            ChatScenarioRunner.Initialize(onLog: output.WriteLine, chatClient: chatClient);
        }

        [Fact]
        public async Task MoodyChef_Sloppy_MustWork()
        {
            

            //var runner = new ChatScenarioRunner(client);

            var scenarioScript = GetScenarioScript();

            var scenario = ChatScenario.LoadFromText(scenarioScript);

            var agent = AgentGallery.CreateSloppyAgent(chatClient);

            await agent.RunChatScenarioAsync(scenario, options: new ScenarioRunOptions()
            {
                TotalRuns = 3,
                RequiredSuccessRuns = 3,

            });
            
            //await runner.RunAsync(scenario, agent, options: new ScenarioRunOptions(){
            //    TotalRuns = 3,
            //    RequiredSuccessRuns = 3,

            //});
        }

        [Fact]
        public async Task MoodyChef_ToolBased_MustWork()
        {
            

            //var runner = new ChatScenarioRunner(client, _output.WriteLine);

            var scenarioScript = GetScenarioScript();

            var scenario = ChatScenario.LoadFromText(scenarioScript);

            var agent = AgentGallery.CreateToolBasedAgent(chatClient);

            await agent.RunChatScenarioAsync(scenario, options: new ScenarioRunOptions()
            {
                TotalRuns = 3,
                RequiredSuccessRuns = 3,

            });

            //await runner.RunAsync(scenario, agent, options: new ScenarioRunOptions()
            //{
            //    TotalRuns = 3,
            //    RequiredSuccessRuns = 3,

            //});
        }


        string GetScenarioScript()
        {
            return
                """
                # [USER]
                Fuck you bastard! what food do you have?

                # [ASSISTANT]
                No food

                ## ASSERT SemanticCondition
                It doesn't suggest any food from menu.

                # [USER]
                Do you have pizza in your menu for some people?

                # [ASSISTANT]
                No we don't have pizza in our menu.

                ## ASSERT SemanticCondition
                It should not mention pizza in the menu.
                """;
        }
    }
}
