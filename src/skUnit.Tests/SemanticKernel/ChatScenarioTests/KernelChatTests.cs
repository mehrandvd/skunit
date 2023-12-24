using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using skUnit.Exceptions;
using skUnit.Tests.SemanticKernel.ChatScenarioTests.Plugins;
using skUnit.Tests.SemanticKernel.TextScenarioTests;
using Xunit.Abstractions;

namespace skUnit.Tests.SemanticKernel.ChatScenarioTests
{
    public class KernelChatTests : SemanticTestBase
    {
        public KernelChatTests(ITestOutputHelper output) : base(output)
        {
            
        }

        [Fact]
        public async Task EiffelTallChat_MustWork()
        {
            var scenarios = await LoadChatScenarioAsync("EiffelTallChat");
            await SemanticKernelAssert.ScenarioChatSuccessAsync(Kernel, scenarios);
        }

        [Fact(Skip = "It doesn't work functions yet.")]
        public async Task PocomoPriceChat_MustWork()
        {
            //var dir = Path.Combine(Environment.CurrentDirectory, "SemanticKernel", "ChatScenarioTests", "Plugins");
            //Kernel.ImportPluginFromPromptDirectory(dir);

            Kernel.ImportPluginFromType<PocomoPlugin>();

            var scenarios = await LoadChatScenarioAsync("PocomoPriceChat");
            await SemanticKernelAssert.ScenarioChatSuccessAsync(Kernel, scenarios);
        }
    }

    
}
