using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using skUnit.Models;
using skUnit.Parsers;

namespace skUnit.Tests.TestCaseTests
{
    public class TestCaseParseTests
    {
        [Fact]
        public void TestCaseParse_Complex_MustWork()
        {
            var testCaseText = """
                # TEST AngryBastard

                ## PARAMETER: input
                
                # Introduction
                You are such a bastard

                # Conclusion
                Fuck off!

                ## PARAMETER: options
                angry, happy

                ## ASSERT
                The sentiment is angry
                """;

            var testCases = TextTestParser.Parse(testCaseText, "");

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

            Assert.True(test.Asserts.Any());
            Assert.IsType<KernelSemanticAssert>(test.Asserts.First());
            Assert.Contains("sentiment", ((KernelSemanticAssert)test.Asserts.First()).Assert);
        }

        [Fact]
        public void TestCaseParse_Light_MustWork()
        {
            var testCaseText = """
                # Introduction
                You are such a bastard
                
                # Conclusion
                Fuck off!
                
                ## ASSERT
                The sentiment is angry
                """;

            var testCases = TextTestParser.Parse(testCaseText, "");

            Assert.NotEmpty(testCases);

            var test = testCases.First();

            Assert.Equal(testCaseText, test.RawText);

            Assert.True(test.Arguments.ContainsKey("input"));
            Assert.True(test.Arguments["input"]?.Contains("Introduction"));
            Assert.True(test.Arguments["input"]?.Contains("Conclusion"));
            Assert.True(test.Arguments["input"]?.Contains("Fuck off!"));

            Assert.True(test.Asserts.Any());
            Assert.IsType<KernelSemanticAssert>(test.Asserts.First());
            Assert.Contains("sentiment", ((KernelSemanticAssert)test.Asserts.First()).Assert);
        }


        [Fact]
        public void TestCaseParse_Multiple_MustWork()
        {
            var testCaseText = """
                # Introduction
                You are such a bastard
                
                # Conclusion
                Fuck off!
                
                ## ASSERT
                The sentiment is angry

                -------------------

                # TEST AngryBastard

                ## PARAMETER: input

                # Introduction
                You are such a bastard

                # Conclusion
                Fuck off!

                ## ASSERT
                The sentiment is angry

                """;

            var testCases = TextTestParser.Parse(testCaseText, "");

            Assert.Equal(2, testCases.Count);

            var first = testCases[0];

            //Assert.Equal(testCaseText, first.RawText);

            Assert.True(first.Arguments.ContainsKey("input"));
            Assert.True(first.Arguments["input"]?.Contains("Introduction"));
            Assert.True(first.Arguments["input"]?.Contains("Conclusion"));
            Assert.True(first.Arguments["input"]?.Contains("Fuck off!"));

            Assert.True(first.Asserts.Any());
            Assert.IsType<KernelSemanticAssert>(first.Asserts.First());
            Assert.Contains("sentiment", ((KernelSemanticAssert)first.Asserts.First()).Assert);


            var second = testCases[1];

            //Assert.Equal(testCaseText, second.RawText);

            Assert.True(second.Arguments.ContainsKey("input"));
            Assert.True(second.Arguments["input"]?.Contains("Introduction"));
            Assert.True(second.Arguments["input"]?.Contains("Conclusion"));
            Assert.True(second.Arguments["input"]?.Contains("Fuck off!"));

            Assert.True(second.Asserts.Any());
            Assert.IsType<KernelSemanticAssert>(second.Asserts.First());
            Assert.Contains("sentiment", ((KernelSemanticAssert)second.Asserts.First()).Assert);
        }
    }
}
