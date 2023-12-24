using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using skUnit.Parsers;
using skUnit.Parsers.Assertions;

namespace skUnit.Tests.TestCaseTests
{
    public class ParseScenarioTests
    {
        [Fact]
        public void ParseScenario_Complex_MustWork()
        {
            var testCaseText = """"
                # TEST AngryBastard

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

            Assert.Equal("AngryBastard", test.Description);
            Assert.Equal(testCaseText, test.RawText);
            
            Assert.True(test.Arguments.ContainsKey("input"));
            Assert.True(test.Arguments["input"]?.Contains("Introduction"));
            Assert.True(test.Arguments["input"]?.Contains("Conclusion"));
            Assert.True(test.Arguments["input"]?.Contains("Fuck off!"));

            Assert.True(test.Arguments.ContainsKey("options"));
            Assert.True(test.Arguments["options"]?.Contains("angry"));
            Assert.True(test.Arguments["options"]?.Contains("happy"));

            Assert.True(test.Asserts.OfType<HasConditionAssertion>().Any());
            var hasCondition = test.Asserts.OfType<HasConditionAssertion>().First();
            Assert.Contains("sentiment", hasCondition.Condition);

            Assert.True(test.Asserts.OfType<AreSameAssertion>().Any());
            var areSame = test.Asserts.OfType<AreSameAssertion>().First();
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

            Assert.True(test.Arguments.ContainsKey("input"));
            Assert.True(test.Arguments["input"]?.Contains("Introduction"));
            Assert.True(test.Arguments["input"]?.Contains("Conclusion"));
            Assert.True(test.Arguments["input"]?.Contains("Fuck off!"));

            Assert.True(test.Asserts.OfType<HasConditionAssertion>().Any());
            var hasCondition = test.Asserts.OfType<HasConditionAssertion>().First();
            Assert.Contains("sentiment", hasCondition.Condition);

            Assert.True(test.Asserts.OfType<AreSameAssertion>().Any());
            var areSame = test.Asserts.OfType<AreSameAssertion>().First();
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

            Assert.True(first.Arguments.ContainsKey("input"));
            Assert.True(first.Arguments["input"]?.Contains("Introduction"));
            Assert.True(first.Arguments["input"]?.Contains("Conclusion"));
            Assert.True(first.Arguments["input"]?.Contains("Fuck off!"));

            Assert.True(first.Asserts.OfType<HasConditionAssertion>().Any());
            Assert.Contains("sentiment", first.Asserts.OfType<HasConditionAssertion>().First().Condition);

            Assert.True(first.Asserts.OfType<AreSameAssertion>().Any());
            Assert.Contains("angry", first.Asserts.OfType<AreSameAssertion>().First().ExpectedAnswer);


            var second = testCases[1];

            //Assert.Equal(testCaseText, second.RawText);

            Assert.True(second.Arguments.ContainsKey("input"));
            Assert.True(second.Arguments["input"]?.Contains("Introduction"));
            Assert.True(second.Arguments["input"]?.Contains("Conclusion"));
            Assert.True(second.Arguments["input"]?.Contains("Fuck off!"));

            Assert.True(second.Asserts.OfType<HasConditionAssertion>().Any());
            Assert.Contains("sentiment", second.Asserts.OfType<HasConditionAssertion>().First().Condition);

            Assert.True(second.Asserts.OfType<AreSameAssertion>().Any());
            Assert.Contains("angry", second.Asserts.OfType<AreSameAssertion>().First().ExpectedAnswer);
        }
    }
}
