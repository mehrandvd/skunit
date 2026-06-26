using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using skUnit.Exceptions;
using skUnit.Runners;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit.Tests.AssertionTests
{
    public class JsonCheckAssertionTests
    {
        [Fact]
        public async Task JasonCheck_MustWork()
        {
            var assertion = new JsonCheckAssertion();

            assertion.ParseSpec("""
                {
                    "name": ["EQUAL", "Mehran"],
                    "address": ["Contain", "Tehran, Vanak"]
                }
                """);

            Assert.NotNull(assertion.JsonCheck);

            await assertion.Assert(
                A.Fake<SemanticEvaluator>(),
                """
                {
                    "name": "Mehran",
                    "address": "The address is in Vanak area of Tehran"
                }
                """, cancellationToken: TestContext.Current.CancellationToken);
        }

        [Fact]
        public async Task JasonCheck_BackQuote_MustWork()
        {
            var assertion = new JsonCheckAssertion();

            assertion.ParseSpec("""
                ```json
                {
                    "name": ["EQUAL", "Mehran"],
                    "address": ["Contain", "Tehran, Vanak"]
                }
                ```
                """);

            Assert.NotNull(assertion.JsonCheck);

            await assertion.Assert(
                A.Fake<SemanticEvaluator>(),
                """
                {
                    "name": "Mehran",
                    "address": "The address is in Vanak area of Tehran"
                }
                """, cancellationToken: TestContext.Current.CancellationToken);
        }

        [Fact]
        public async Task JasonCheck_Contains_Fail_MustWork()
        {
            var assertion = new JsonCheckAssertion();

            assertion.ParseSpec("""
                {
                    "name": ["EQUAL", "Mehran"],
                    "address": ["Contain", "Tehran, Gisha"]
                }
                """);

            Assert.NotNull(assertion.JsonCheck);

            await Assert.ThrowsAsync<SemanticAssertException>(() => assertion.Assert(
                A.Fake<SemanticEvaluator>(),
                """
                {
                    "name": "Mehran",
                    "address": "The address is in Vanak area of Tehran"
                }
                """, cancellationToken: TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task JasonCheck_PropertyNotFound_MustWork()
        {
            var assertion = new JsonCheckAssertion();

            assertion.ParseSpec("""
                {
                    "name": ["EQUAL", "Mehran"],
                    "address": ["Contain", "Tehran, Gisha"]
                }
                """);

            Assert.NotNull(assertion.JsonCheck);

            await Assert.ThrowsAsync<SemanticAssertException>(() => assertion.Assert(
                A.Fake<SemanticEvaluator>(),
                """
                {
                    "name": "Mehran",
                }
                """, cancellationToken: TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task JasonCheck_InvalidCheck_Throws()
        {
            var assertion = new JsonCheckAssertion();

            assertion.ParseSpec("""
                {
                    "name": ["EQUAL", "Mehran", "Haha"],
                    "address": ["Contain", "Tehran, Gisha"]
                }
                """);

            Assert.NotNull(assertion.JsonCheck);

            await Assert.ThrowsAsync<InvalidOperationException>(() => assertion.Assert(
                A.Fake<SemanticEvaluator>(),
                """
                {
                    "name": "Mehran",
                }
                """, cancellationToken: TestContext.Current.CancellationToken));
        }
    }
}
