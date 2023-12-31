﻿using skUnit.Scenarios;
using skUnit.Scenarios.Parsers;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit.Tests.ScenarioTests
{
    public class ParseInvokeScenarioTests
    {
        [Fact]
        public void ParseScenario_Complex_MustWork()
        {
            var testCaseText = """
                # SCENARIO AngryBastard

                ## PROMPT
                Get intent of input with options.

                ## PARAMETER input
                
                # Introduction
                You are such a bastard

                # Conclusion
                Fuck off!

                ## PARAMETER options
                angry, happy

                ## ANSWER SemanticSimilar
                The sentiment is angry
                
                ### CHECK SemanticCondition
                Expresses a sentiment
                """;

            var scenarios = InvocationScenario.LoadFromText(testCaseText, "");

            Assert.NotEmpty(scenarios);

            var test = scenarios.First();

            Assert.Equal("Get intent of input with options.", test.Prompt);
            Assert.Equal("AngryBastard", test.Description);
            Assert.Equal("The sentiment is angry", test.ExpectedAnswer);
            Assert.Equal(testCaseText, test.RawText);
            
            Assert.True(test.Parameters.ContainsKey("input"));
            Assert.True(test.Parameters["input"]?.Contains("Introduction"));
            Assert.True(test.Parameters["input"]?.Contains("Conclusion"));
            Assert.True(test.Parameters["input"]?.Contains("Fuck off!"));

            Assert.True(test.Parameters.ContainsKey("options"));
            Assert.True(test.Parameters["options"]?.Contains("angry"));
            Assert.True(test.Parameters["options"]?.Contains("happy"));

            Assert.True(test.Assertions.OfType<HasConditionAssertion>().Any());
            var hasCondition = test.Assertions.OfType<HasConditionAssertion>().First();
            Assert.Contains("sentiment", hasCondition.Condition);

            Assert.True(test.Assertions.OfType<AreSimilarAssertion>().Any());
            Assert.Contains("angry", test.Assertions.OfType<AreSimilarAssertion>().First().ExpectedAnswer);
        }

        [Fact]
        public void ParseScenario_Light_MustWork()
        {
            var testCaseText = """
                # Introduction
                You are such a bastard
                
                # Conclusion
                Fuck off!
                
                ## ANSWER
                The sentiment is angry
                
                ### CHECK

                ### CHECK SemanticCondition
                Expresses a sentiment
                """;

            var scenarios = InvocationScenario.LoadFromText(testCaseText, "");

            Assert.NotEmpty(scenarios);

            var test = scenarios.First();

            Assert.Equal(testCaseText, test.RawText);

            Assert.True(test.Parameters.ContainsKey("input"));
            Assert.True(test.Parameters["input"]?.Contains("Introduction"));
            Assert.True(test.Parameters["input"]?.Contains("Conclusion"));
            Assert.True(test.Parameters["input"]?.Contains("Fuck off!"));

            Assert.True(test.Assertions.OfType<HasConditionAssertion>().Any());
            Assert.Contains("sentiment", test.Assertions.OfType<HasConditionAssertion>().First().Condition);

            Assert.True(test.Assertions.OfType<AreSimilarAssertion>().Any());
            Assert.Contains("angry", test.Assertions.OfType<AreSimilarAssertion>().First().ExpectedAnswer);
        }


        [Fact]
        public void ParseScenario_Multiple_MustWork()
        {
            var testCaseText = """
                # Introduction
                You are such a bastard
                
                # Conclusion
                Fuck off!
                
                ## ANSWER SemanticSimilar
                The sentiment is angry

                ### CHECK SemanticCondition
                Expresses a sentiment

                -------------------

                # SCENARIO AngryBastard

                ## PARAMETER input

                # Introduction
                You are such a bastard

                # Conclusion
                Fuck off!

                ## ANSWER
                The sentiment is angry
                
                ### CHECK SemanticCondition
                Expresses a sentiment
                """;

            var testCases = InvocationScenario.LoadFromText(testCaseText, "");

            Assert.Equal(2, testCases.Count);

            var first = testCases[0];

            //Assert.Equal(testCaseText, first.RawText);

            Assert.True(first.Parameters.ContainsKey("input"));
            Assert.True(first.Parameters["input"]?.Contains("Introduction"));
            Assert.True(first.Parameters["input"]?.Contains("Conclusion"));
            Assert.True(first.Parameters["input"]?.Contains("Fuck off!"));

            Assert.True(first.Assertions.OfType<HasConditionAssertion>().Any());
            Assert.Contains("sentiment", first.Assertions.OfType<HasConditionAssertion>().First().Condition);

            Assert.True(first.Assertions.OfType<AreSimilarAssertion>().Any());
            Assert.Contains("angry", first.Assertions.OfType<AreSimilarAssertion>().First().ExpectedAnswer);


            var second = testCases[1];

            //Assert.Equal(testCaseText, second.RawText);

            Assert.True(second.Parameters.ContainsKey("input"));
            Assert.True(second.Parameters["input"]?.Contains("Introduction"));
            Assert.True(second.Parameters["input"]?.Contains("Conclusion"));
            Assert.True(second.Parameters["input"]?.Contains("Fuck off!"));

            Assert.True(second.Assertions.OfType<HasConditionAssertion>().Any());
            Assert.Contains("sentiment", second.Assertions.OfType<HasConditionAssertion>().First().Condition);

            Assert.False(second.Assertions.OfType<AreSimilarAssertion>().Any());
        }

        [Fact]
        public void ParseScenario_SpecialId_MustWork()
        {
            var testCaseText = """
                # ~ SCENARIO AngryBastard

                ## ~ PROMPT
                Get intent of input with options.

                ## ~ PARAMETER input
                
                # ~ Introduction
                You are such a bastard

                # ~ Conclusion
                Fuck off!

                ## ~ PARAMETER options
                angry, happy

                ## ~ ANSWER SemanticSimilar
                The sentiment is angry
                
                ### ~ CHECK SemanticCondition
                Expresses a sentiment
                
                -------------------
                
                # sk SCENARIO AngryBastard
                
                ## sk PROMPT
                Get intent of input with options.
                
                ## sk PARAMETER input
                
                # sk Introduction
                You are such a bastard
                
                # sk Conclusion
                Fuck off!
                
                ## sk PARAMETER options
                angry, happy
                
                ## sk ANSWER SemanticSimilar
                The sentiment is angry
                
                ### sk CHECK SemanticCondition
                Expresses a sentiment
                """;

            var scenarios = InvocationScenario.LoadFromText(testCaseText, "");

            Assert.NotEmpty(scenarios);

            var first = scenarios.First();

            Assert.Equal("Get intent of input with options.", first.Prompt);
            Assert.Equal("AngryBastard", first.Description);
            Assert.Equal("The sentiment is angry", first.ExpectedAnswer);

            Assert.True(first.Parameters.ContainsKey("input"));
            Assert.True(first.Parameters["input"]?.Contains("Introduction"));
            Assert.True(first.Parameters["input"]?.Contains("Conclusion"));
            Assert.True(first.Parameters["input"]?.Contains("Fuck off!"));

            Assert.True(first.Parameters.ContainsKey("options"));
            Assert.True(first.Parameters["options"]?.Contains("angry"));
            Assert.True(first.Parameters["options"]?.Contains("happy"));

            Assert.True(first.Assertions.OfType<HasConditionAssertion>().Any());
            var hasCondition = first.Assertions.OfType<HasConditionAssertion>().First();
            Assert.Contains("sentiment", hasCondition.Condition);

            Assert.True(first.Assertions.OfType<AreSimilarAssertion>().Any());
            Assert.Contains("angry", first.Assertions.OfType<AreSimilarAssertion>().First().ExpectedAnswer);


            var second = scenarios[1];

            Assert.Equal("Get intent of input with options.", second.Prompt);
            Assert.Equal("AngryBastard", second.Description);
            Assert.Equal("The sentiment is angry", second.ExpectedAnswer);

            Assert.True(second.Parameters.ContainsKey("input"));
            Assert.True(second.Parameters["input"]?.Contains("Introduction"));
            Assert.True(second.Parameters["input"]?.Contains("Conclusion"));
            Assert.True(second.Parameters["input"]?.Contains("Fuck off!"));

            Assert.True(second.Parameters.ContainsKey("options"));
            Assert.True(second.Parameters["options"]?.Contains("angry"));
            Assert.True(second.Parameters["options"]?.Contains("happy"));

            Assert.True(second.Assertions.OfType<HasConditionAssertion>().Any());
            Assert.Contains("sentiment", second.Assertions.OfType<HasConditionAssertion>().First().Condition);

            Assert.True(second.Assertions.OfType<AreSimilarAssertion>().Any());
            Assert.Contains("angry", second.Assertions.OfType<AreSimilarAssertion>().First().ExpectedAnswer);
        }
    }
}
