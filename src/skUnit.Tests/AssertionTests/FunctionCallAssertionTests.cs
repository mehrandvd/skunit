using FakeItEasy;
using Microsoft.Extensions.AI;
using skUnit.Exceptions;
using skUnit.Runners;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit.Tests.AssertionTests
{
    public class FunctionCallAssertionTests
    {
        [Fact]
        public void FunctionCall_MustWork()
        {
            var assertion = new FunctionCallAssertion();

            assertion.ParseSpec("""
                {
                    "function_name": "test_function",
                }
                """);

            Assert.NotNull(assertion.FunctionCallJson);
            Assert.Equal("test_function", assertion.Spec?.FunctionName);

        }

        [Fact]
        public void FunctionCall_BackQuote_MustWork()
        {
            var assertion = new FunctionCallAssertion();

            assertion.ParseSpec("""
                ```json
                {
                    "function_name": "test_function",
                }
                ```
                """);

            Assert.NotNull(assertion.FunctionCallJson);
            Assert.Equal("test_function", assertion.Spec?.FunctionName);

        }

        [Fact]
        public async Task FunctionCall_ArgumentAssertion_StringLiteral()
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
                        new FunctionResultContent("call-id-1", "result"),
                    }
                },
            };

            var assertion = new FunctionCallAssertion();
            assertion.ParseSpec("""
                                        ```json
                                        {
                                            "function_name": "test_function",
                                            "arguments": {
                                                "arg1": "value1"
                                            }
                                        }
                                        ```
                                        """);

            await assertion.Assert(A.Fake<SemanticEvaluator>(), new ChatResponse(history), history, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Single(assertion.FunctionArguments);
            Assert.Equal(typeof(EqualsAssertion), assertion.FunctionArguments["arg1"].GetType());
        }

        [Fact]
        public async Task FunctionCall_ArgumentAssertion_IsAnyOf()
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
                        new FunctionResultContent("call-id-1", "result"),
                    }
                },
            };

            var assertion = new FunctionCallAssertion();
            assertion.ParseSpec("""
                ```json
                {
                    "function_name": "test_function",
                    "arguments": {
                        "arg1": ["IsAnyOf", "value1", "value2", "value3"]
                    }
                }
                ```
                """);

            await assertion.Assert(A.Fake<SemanticEvaluator>(), new ChatResponse(history), history, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Single(assertion.FunctionArguments);
            Assert.Equal(typeof(IsAnyOfAssertion), assertion.FunctionArguments["arg1"].GetType());
        }

        [Fact]
        public async Task FunctionCall_ArgumentAssertion_SeveralArguments()
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
                        new FunctionResultContent("call-id-1", "result"),
                    }
                },
            };

            var assertion = new FunctionCallAssertion();
            assertion.ParseSpec("""
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

            await assertion.Assert(A.Fake<SemanticEvaluator>(), new ChatResponse(history), history, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(expected: 3, assertion.FunctionArguments.Count);
            Assert.Equal(typeof(IsAnyOfAssertion), assertion.FunctionArguments["arg1"].GetType());
            Assert.Equal(typeof(ContainsAnyAssertion), assertion.FunctionArguments["arg2"].GetType());
            Assert.Equal(typeof(NotEmptyAssertion), assertion.FunctionArguments["arg3"].GetType());
        }

        [Fact]
        public async Task FunctionCall_ArgumentAssertion_DoesNotMatch()
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
                            { "arg1", "value" }
                        }),
                        new FunctionResultContent("call-id-1", "result"),
                    }
                },
            };

            var assertion = new FunctionCallAssertion();
            assertion.ParseSpec("""
                ```json
                {
                    "function_name": "test_function",
                    "arguments": {
                        "arg1": ["IsAnyOf", "value1", "value2", "value3"]
                    }
                }
                ```
                """);

            var exception = await Assert.ThrowsAsync<SemanticAssertException>(() =>
            {
                return assertion.Assert(A.Fake<SemanticEvaluator>(), new ChatResponse(history), history, cancellationToken: TestContext.Current.CancellationToken);
            });

            Assert.Equal("Text is not equal to any of these: 'value1, value2, value3'", exception.Message);
            Assert.Single(assertion.FunctionArguments);
            Assert.Equal(typeof(IsAnyOfAssertion), assertion.FunctionArguments["arg1"].GetType());
        }
    }
}
