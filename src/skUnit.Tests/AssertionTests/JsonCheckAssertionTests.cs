using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using skUnit.Exceptions;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit.Tests.AssertionTests
{
    public class JsonCheckAssertionTests
    {
        [Fact]
        public async Task JasonCheck_MustWork()
        {
            var assertion = new JsonCheckAssertion();

            assertion.SetJsonAssertText("""
                {
                    "name": ["EQUAL", "Mehran"],
                    "address": ["Contain", "Tehran, Vanak"]
                }
                """);

            Assert.NotNull(assertion.JsonCheck);

            await assertion.Assert(null, """
                                         {
                                             "name": "Mehran",
                                             "address": "The address is in Vanak area of Tehran"
                                         }
                                         """);
        }

        [Fact]
        public async Task JasonCheck_BackQuote_MustWork()
        {
            var assertion = new JsonCheckAssertion();

            assertion.SetJsonAssertText("""
                ```json
                {
                    "name": ["EQUAL", "Mehran"],
                    "address": ["Contain", "Tehran, Vanak"]
                }
                ```
                """);

            Assert.NotNull(assertion.JsonCheck);

            await assertion.Assert(null, """
                                         {
                                             "name": "Mehran",
                                             "address": "The address is in Vanak area of Tehran"
                                         }
                                         """);
        }

        [Fact]
        public async Task JasonCheck_Contains_Fail_MustWork()
        {
            var assertion = new JsonCheckAssertion();

            assertion.SetJsonAssertText("""
                {
                    "name": ["EQUAL", "Mehran"],
                    "address": ["Contain", "Tehran, Gisha"]
                }
                """);

            Assert.NotNull(assertion.JsonCheck);

            await Assert.ThrowsAsync<SemanticAssertException>( () => assertion.Assert(null, """
                {
                    "name": "Mehran",
                    "address": "The address is in Vanak area of Tehran"
                }
                """));
        }

        [Fact]
        public async Task JasonCheck_PropertyNotFound_MustWork()
        {
            var assertion = new JsonCheckAssertion();

            assertion.SetJsonAssertText("""
                {
                    "name": ["EQUAL", "Mehran"],
                    "address": ["Contain", "Tehran, Gisha"]
                }
                """);

            Assert.NotNull(assertion.JsonCheck);

            await Assert.ThrowsAsync<SemanticAssertException>(() => assertion.Assert(null, """
                {
                    "name": "Mehran",
                }
                """));
        }

        [Fact]
        public async Task JasonCheck_InvalidCheck_Throws()
        {
            var assertion = new JsonCheckAssertion();

            assertion.SetJsonAssertText("""
                {
                    "name": ["EQUAL", "Mehran", "Haha"],
                    "address": ["Contain", "Tehran, Gisha"]
                }
                """);

            Assert.NotNull(assertion.JsonCheck);

            await Assert.ThrowsAsync<InvalidOperationException>(() => assertion.Assert(null, """
                {
                    "name": "Mehran",
                }
                """));
        }
    }
}
