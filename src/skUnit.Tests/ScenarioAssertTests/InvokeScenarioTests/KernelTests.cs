using skUnit.Exceptions;
using skUnit.Tests.Infrastructure;
using Xunit.Abstractions;

namespace skUnit.Tests.ScenarioAssertTests.InvokeScenarioTests
{
    public class ScenarioAssertInvokeTests : SemanticTestBase
    {
        public ScenarioAssertInvokeTests(ITestOutputHelper output) : base(output)
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
