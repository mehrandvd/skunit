using Microsoft.Extensions.AI;
using skUnit.Exceptions;
using skUnit.Scenarios.Parsers.Assertions;
using skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

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

        [Fact]
        public async Task FunctionCall_ArgumentCondition_IsAnyOf()
        {
            var history = new List<ChatMessage>()
            {
                new ChatMessage()
                {
                    Role = ChatRole.Tool,
                    Contents = new List<AIContent>()
                    {
                        new FunctionCallContent("call-id-1", "test_function", new Dictionary<string, object?>()
                        {
                            { "arg1", "value1" }
                        }),
                        new FunctionResultContent("call-id-1", "test_function", "result"),
                    }
                },
            };

            var assertion = new FunctionCallAssertion();
            assertion.SetJsonAssertText("""
                ```json
                {
                    "function_name": "test_function",
                    "arguments": {
                        "arg1": ["IsAnyOf", "value1", "value2", "value3"]
                    }
                }
                ```
                """);

            await assertion.Assert(null, null, history);

            Assert.Single(assertion.FunctionArguments);
            Assert.Equal(typeof(IsAnyOfArgumentCondition), assertion.FunctionArguments["arg1"].GetType());
        }

        [Fact]
        public async Task FunctionCall_ArgumentCondition_SeveralArguments()
        {
            var history = new List<ChatMessage>()
            {
                new ChatMessage()
                {
                    Role = ChatRole.Tool,
                    Contents = new List<AIContent>()
                    {
                        new FunctionCallContent("call-id-1", "test_function", new Dictionary<string, object?>()
                        {
                            { "arg1", "value1" },
                            { "arg2", "Actual value: value1" },
                            { "arg3", "value" }
                        }),
                        new FunctionResultContent("call-id-1", "test_function", "result"),
                    }
                },
            };

            var assertion = new FunctionCallAssertion();
            assertion.SetJsonAssertText("""
                ```json
                {
                    "function_name": "test_function",
                    "arguments": {
                        "arg1": ["IsAnyOf", "value1", "value2", "value3"],
                        "arg2": ["ContainsAny", "value1", "value2", "value3"],
                        "arg3": ["NotEmpty"],
                    }
                }
                ```
                """);

            await assertion.Assert(null, null, history);

            Assert.Equal(expected: 3, assertion.FunctionArguments.Count);
            Assert.Equal(typeof(IsAnyOfArgumentCondition), assertion.FunctionArguments["arg1"].GetType());
            Assert.Equal(typeof(ContainsAnyArgumentCondition), assertion.FunctionArguments["arg2"].GetType());
            Assert.Equal(typeof(NotEmptyArgumentCondition), assertion.FunctionArguments["arg3"].GetType());
        }

        [Fact]
        public async Task FunctionCall_ArgumentCondition_DoesNotMatch()
        {
            var history = new List<ChatMessage>()
            {
                new ChatMessage()
                {
                    Role = ChatRole.Tool,
                    Contents = new List<AIContent>()
                    {
                        new FunctionCallContent("call-id-1", "test_function", new Dictionary<string, object?>()
                        {
                            { "arg1", "value1" }
                        }),
                        new FunctionResultContent("call-id-1", "test_function", "result"),
                    }
                },
            };

            var assertion = new FunctionCallAssertion();
            assertion.SetJsonAssertText("""
                ```json
                {
                    "function_name": "test_function",
                    "arguments": {
                        "arg1": ["Equals", "value2"]
                    }
                }
                ```
                """);

            var exception = await Assert.ThrowsAsync<SemanticAssertException>(() =>
            {
                return assertion.Assert(null, null, history);
            });

            Assert.Equal("Argument `arg1` is expected to satisfy condition `Equals`, but it does not.\nActual value: `value1`", exception.Message);
            Assert.Single(assertion.FunctionArguments);
            Assert.Equal(typeof(EqualsArgumentCondition), assertion.FunctionArguments["arg1"].GetType());
        }
    }
}
