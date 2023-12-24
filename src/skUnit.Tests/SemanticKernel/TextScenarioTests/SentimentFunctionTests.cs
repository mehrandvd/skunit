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
    public class SentimentFunctionTests : SemanticTest
    {
        protected KernelFunction SentimentFunction { get; set; }

        public SentimentFunctionTests(ITestOutputHelper output) : base(output)
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
            var scenarios = await LoadScenarioAsync("TextScenarioTests.Samples.SentimentAngry_Complex");
            await SemanticKernelAssert.ScenarioSuccessAsync(Kernel, SentimentFunction, scenarios);
        }

        [Fact]
        public async Task Angry_False_MustWork()
        {
            var scenarios = await LoadScenarioAsync("TextScenarioTests.Samples.SentimentHappy");
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
