using skUnit.Exceptions;
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

        //[Fact]
        //public async Task Angry_False_MustWork()
        //{
        //    var scenarios = await LoadChatScenarioAsync("SentimentHappy");
        //    await SemanticKernelAssert.ScenarioThrowsAsync<SemanticAssertException>(Kernel, scenarios);

        //    //foreach (var scenario in scenarios)
        //    //{
        //    //    var exception = await Assert.ThrowsAsync<SemanticAssertException>(() => SemanticKernelAssert.TestScenarioOnFunction(Kernel, SentimentFunction, scenario));
        //    //    Output.WriteLine($"""
        //    //        EXCEPTION MESSAGE:
        //    //        {exception.Message}
        //    //        """);
        //    //}
        //}


    }

    
}
