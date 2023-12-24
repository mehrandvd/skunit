using skUnit.Scenarios.Parsers;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit.Tests.ScenarioTests
{
    public class ParseScenarioTests
    {
        [Fact]
        public void ParseScenario_Complex_MustWork()
        {
            var testCaseText = """"
                # TEST AngryBastard

                ## PROMPT
                Get intent of input with options.

                ## PARAMETER input
                
                # Introduction
                You are such a bastard

                # Conclusion
                Fuck off!

                ## PARAMETER options
                angry, happy

                ## ANSWER
                The sentiment is angry
                """

                ## ANSWER CONDITION
                Expresses a sentiment
                """";

            var testCases = TextScenarioParser.Parse(testCaseText, "");

            Assert.NotEmpty(testCases);

            var test = testCases.First();

            Assert.Equal("Get intent of input with options.", test.Prompt);
            Assert.Equal("AngryBastard", test.Description);
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

            Assert.True(test.Assertions.OfType<AreSameAssertion>().Any());
            var areSame = test.Assertions.OfType<AreSameAssertion>().First();
            Assert.Contains("angry", areSame.ExpectedAnswer);
        }

        [Fact]
        public void ParseScenario_Light_MustWork()
        {
            var testCaseText = """"
                # Introduction
                You are such a bastard
                
                # Conclusion
                Fuck off!
                
                ## ANSWER
                The sentiment is angry
                """
                
                ## ANSWER CONDITION
                Expresses a sentiment
                """";

            var testCases = TextScenarioParser.Parse(testCaseText, "");

            Assert.NotEmpty(testCases);

            var test = testCases.First();

            Assert.Equal(testCaseText, test.RawText);

            Assert.True(test.Parameters.ContainsKey("input"));
            Assert.True(test.Parameters["input"]?.Contains("Introduction"));
            Assert.True(test.Parameters["input"]?.Contains("Conclusion"));
            Assert.True(test.Parameters["input"]?.Contains("Fuck off!"));

            Assert.True(test.Assertions.OfType<HasConditionAssertion>().Any());
            var hasCondition = test.Assertions.OfType<HasConditionAssertion>().First();
            Assert.Contains("sentiment", hasCondition.Condition);

            Assert.True(test.Assertions.OfType<AreSameAssertion>().Any());
            var areSame = test.Assertions.OfType<AreSameAssertion>().First();
            Assert.Contains("angry", areSame.ExpectedAnswer);
        }


        [Fact]
        public void ParseScenario_Multiple_MustWork()
        {
            var testCaseText = """"
                # Introduction
                You are such a bastard
                
                # Conclusion
                Fuck off!
                
                ## ANSWER
                The sentiment is angry
                """
                
                ## ANSWER CONDITION
                Expresses a sentiment

                -------------------

                # TEST AngryBastard

                ## PARAMETER input

                # Introduction
                You are such a bastard

                # Conclusion
                Fuck off!

                ## ANSWER
                The sentiment is angry
                """
                
                ## ANSWER CONDITION
                Expresses a sentiment
                """";

            var testCases = TextScenarioParser.Parse(testCaseText, "");

            Assert.Equal(2, testCases.Count);

            var first = testCases[0];

            //Assert.Equal(testCaseText, first.RawText);

            Assert.True(first.Parameters.ContainsKey("input"));
            Assert.True(first.Parameters["input"]?.Contains("Introduction"));
            Assert.True(first.Parameters["input"]?.Contains("Conclusion"));
            Assert.True(first.Parameters["input"]?.Contains("Fuck off!"));

            Assert.True(first.Assertions.OfType<HasConditionAssertion>().Any());
            Assert.Contains("sentiment", first.Assertions.OfType<HasConditionAssertion>().First().Condition);

            Assert.True(first.Assertions.OfType<AreSameAssertion>().Any());
            Assert.Contains("angry", first.Assertions.OfType<AreSameAssertion>().First().ExpectedAnswer);


            var second = testCases[1];

            //Assert.Equal(testCaseText, second.RawText);

            Assert.True(second.Parameters.ContainsKey("input"));
            Assert.True(second.Parameters["input"]?.Contains("Introduction"));
            Assert.True(second.Parameters["input"]?.Contains("Conclusion"));
            Assert.True(second.Parameters["input"]?.Contains("Fuck off!"));

            Assert.True(second.Assertions.OfType<HasConditionAssertion>().Any());
            Assert.Contains("sentiment", second.Assertions.OfType<HasConditionAssertion>().First().Condition);

            Assert.True(second.Assertions.OfType<AreSameAssertion>().Any());
            Assert.Contains("angry", second.Assertions.OfType<AreSameAssertion>().First().ExpectedAnswer);
        }
    }
}
