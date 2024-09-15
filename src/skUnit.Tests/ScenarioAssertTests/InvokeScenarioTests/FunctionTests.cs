using Microsoft.SemanticKernel;
using skUnit.Exceptions;
using skUnit.Tests.Infrastructure;
using Xunit.Abstractions;

namespace skUnit.Tests.ScenarioAssertTests.InvokeScenarioTests
{
    public class FunctionTests : SemanticTestBase
    {
        protected KernelFunction SentimentFunction { get; set; }

        public FunctionTests(ITestOutputHelper output) : base(output)
        {
            var prompt = """
                What is sentiment of this input text, your options are: {{$options}}
                
                [[input text]]
                {{$input}}
                [[end of input text]]
                
                just result the sentiment without any spaces.

                SENTIMENT: 
                """;
            SentimentFunction = Kernel.CreateFunctionFromPrompt(prompt);
        }

        [Fact]
        public async Task Angry_True_MustWork()
        {
            var scenarios = await LoadInvokeScenarioAsync("SentimentAngry_Complex");
            await SemanticKernelAssert.CheckScenarioAsync(Kernel, SentimentFunction, scenarios);
        }

        [Fact]
        public async Task Angry_False_MustWork()
        {
            var scenarios = await LoadInvokeScenarioAsync("SentimentHappy");
            await SemanticKernelAssert.ScenarioThrowsAsync<SemanticAssertException>(Kernel, SentimentFunction, scenarios);

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
