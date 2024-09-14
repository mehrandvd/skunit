using skUnit.Scenarios;
using skUnit;
using Xunit.Abstractions;
using Microsoft.VisualStudio.TestPlatform.Utilities;

namespace Demo.TddRepl.Test
{
    public class BrainTests
    {
        protected SemanticKernelAssert SemanticAssert { get; set; }
        public BrainTests(ITestOutputHelper output)
        {
            var endPoint = Environment.GetEnvironmentVariable("openai-endpoint") ?? throw new InvalidOperationException("No key provided.");
            var apiKey = Environment.GetEnvironmentVariable("openai-api-key") ?? throw new InvalidOperationException("No key provided.");
            var deploymentName = Environment.GetEnvironmentVariable("openai-deployment-name") ?? throw new InvalidOperationException("No key provided.");

            SemanticAssert = new SemanticKernelAssert(deploymentName, endPoint, apiKey, output.WriteLine);
        }

        [Fact]
        public async Task Greeting()
        {
            var brain = new Brain();

            var scenarios = await ChatScenario.LoadFromResourceAsync(@"Demo.TddRepl.Test.Scenarios.01-Greeting.md", GetType().Assembly);
            await SemanticAssert.CheckChatScenarioAsync(scenarios, async history =>
            {
                var result = await brain.GetChatAnswerAsync(history);

                return result?.ToString() ?? string.Empty;
            });
        }

        [Fact]
        public async Task WhoIsMehran_Normal()
        {
            var brain = new Brain();

            var scenarios = await ChatScenario.LoadFromResourceAsync(@"Demo.TddRepl.Test.Scenarios.02-WhoIsMehran-Normal.md", GetType().Assembly);
            await SemanticAssert.CheckChatScenarioAsync(scenarios, async history =>
            {
                var result = await brain.GetChatAnswerAsync(history);

                return result?.ToString() ?? string.Empty;
            });
        }

        [Fact]
        public async Task WhoIsMehran_Angry()
        {
            var brain = new Brain();
            
            var scenarios = await ChatScenario.LoadFromResourceAsync(@"Demo.TddRepl.Test.Scenarios.03-WhoIsMehran-Angry.md", GetType().Assembly);
            await SemanticAssert.CheckChatScenarioAsync(scenarios, async history =>
            {
                var result = await brain.GetChatAnswerAsync(history);

                return result?.ToString() ?? string.Empty;
            });
        }

        [Fact]
        public async Task WhoIsMehran_AngryNormal()
        {
            var brain = new Brain();
            
            var scenarios = await ChatScenario.LoadFromResourceAsync(@"Demo.TddRepl.Test.Scenarios.04-WhoIsMehran-AngryNormal.md", GetType().Assembly);
            await SemanticAssert.CheckChatScenarioAsync(scenarios, async history =>
            {
                var result = await brain.GetChatAnswerAsync(history);

                return result?.ToString() ?? string.Empty;
            });
        }
    }
}