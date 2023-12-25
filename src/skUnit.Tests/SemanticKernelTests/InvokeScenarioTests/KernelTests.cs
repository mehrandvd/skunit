using skUnit.Exceptions;
using Xunit.Abstractions;

namespace skUnit.Tests.SemanticKernelTests.InvokeScenarioTests
{
    public class KernelTests : SemanticTestBase
    {
        public KernelTests(ITestOutputHelper output) : base(output)
        {
            
        }

        [Fact]
        public async Task Angry_True_MustWork()
        {
            var scenarios = await LoadInvokeScenarioAsync("SentimentAngry_Complex");
            await SemanticKernelAssert.CheckScenarioAsync(Kernel, scenarios);
        }

        [Fact]
        public async Task Angry_False_MustWork()
        {
            var scenarios = await LoadInvokeScenarioAsync("SentimentHappy");
            await SemanticKernelAssert.ScenarioThrowsAsync<SemanticAssertException>(Kernel, scenarios);

            //foreach (var scenario in scenarios)
            //{
            //    var exception = await Assert.ThrowsAsync<SemanticAssertException>(() => SemanticKernelAssert.TestScenarioOnFunction(Kernel, SentimentFunction, scenario));
            //    Output.WriteLine($"""
            //        EXCEPTION MESSAGE:
            //        {exception.Message}
            //        """);
            //}
        }


    }

    
}
