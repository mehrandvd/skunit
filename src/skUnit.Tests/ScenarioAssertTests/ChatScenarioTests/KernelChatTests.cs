using Microsoft.SemanticKernel;
using skUnit.Tests.Infrastructure;
using skUnit.Tests.SemanticKernelTests.ChatScenarioTests.Plugins;
using skUnit.Tests.SemanticKernelTests.InvokeScenarioTests;
using Xunit.Abstractions;

namespace skUnit.Tests.ScenarioAssertTests.ChatScenarioTests
{
    public class ScenarioAssertTests : SemanticTestBase
    {
        public ScenarioAssertTests(ITestOutputHelper output) : base(output)
        {
            var func = Kernel.CreateFunctionFromPrompt("""
                [[INPUT]]
                {{$input}}
                [[END OF INPUT]]
                
                Get intent of input. Intent should be one of these options: {{$options}}.

                INTENT:
                """, new PromptExecutionSettings(), "GetIntent");
            Kernel.Plugins.AddFromFunctions("MyPlugin", "", new[] { func });
        }

        [Fact]
        public async Task EiffelTallChat_MustWork()
        {
            var scenarios = await LoadChatScenarioAsync("EiffelTallChat");
            await SemanticKernelAssert.CheckChatScenarioAsync(Kernel, scenarios);
        }

        [Fact(Skip = "It doesn't work functions yet.")]
        public async Task PocomoPriceChat_MustWork()
        {
            //var dir = Path.Combine(Environment.CurrentDirectory, "SemanticKernel", "ChatScenarioTests", "Plugins");
            //Kernel.ImportPluginFromPromptDirectory(dir);

            Kernel.ImportPluginFromType<PocomoPlugin>();

            var scenarios = await LoadChatScenarioAsync("PocomoPriceChat");
            await SemanticKernelAssert.CheckChatScenarioAsync(Kernel, scenarios);
        }
    }

    
}
