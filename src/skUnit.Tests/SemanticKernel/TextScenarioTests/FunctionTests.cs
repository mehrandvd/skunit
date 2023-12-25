using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using skUnit.Exceptions;
using Xunit.Abstractions;

namespace skUnit.Tests.SemanticKernel.TextScenarioTests
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
            var scenarios = await LoadTextScenarioAsync("SentimentAngry_Complex");
            await SemanticKernelAssert.CheckScenarioAsync(Kernel, SentimentFunction, scenarios);
        }

        [Fact]
        public async Task Angry_False_MustWork()
        {
            var scenarios = await LoadTextScenarioAsync("SentimentHappy");
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
