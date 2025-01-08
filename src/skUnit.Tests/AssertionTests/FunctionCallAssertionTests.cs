using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using skUnit.Exceptions;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit.Tests.AssertionTests
{
    public class FunctionCallAssertionTests
    {
        [Fact]
        public async Task FunctionCall_MustWork()
        {
            var assertion = new FunctionCallAssertion();

            assertion.SetJsonAssertText("""
                {
                    "function_name": "test_function",
                }
                """);

            Assert.NotNull(assertion.FunctionCallJson);
            Assert.Equal("test_function", assertion.FunctionName);

        }

        [Fact]
        public async Task FunctionCall_BackQuote_MustWork()
        {
            var assertion = new FunctionCallAssertion();

            assertion.SetJsonAssertText("""
                ```json
                {
                    "function_name": "test_function",
                }
                ```
                """);

            Assert.NotNull(assertion.FunctionCallJson);
            Assert.Equal("test_function", assertion.FunctionName);

        }
    }
}
