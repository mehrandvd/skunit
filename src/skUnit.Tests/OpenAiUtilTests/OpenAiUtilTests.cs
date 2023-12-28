using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using skUnit.Utils;

namespace skUnit.Tests.OpenAiUtilTests
{
    public class OpenAiUtilTests
    {
        [Fact]
        public void ParseJson_SimpleObject()
        {
            var text = """
                {
                    "name": "mehran",
                    "desc": "software"
                }
                """;

            var json = OpenAiUtil.ParseJson<JsonObject>(text);

            Assert.NotNull(json);
        }

        [Fact]
        public void ParseJson_SimpleArray()
        {
            var text = """
                [
                    {
                        "name": "mehran"
                    }  
                ]
                """;

            var json = OpenAiUtil.ParseJson<JsonArray>(text);

            Assert.NotNull(json);
        }

        [Theory]
        [InlineData("\"")]
        [InlineData("'")]
        [InlineData("`")]
        [InlineData("```")]
        public void ParseJson_Quotes(string quote)
        {
            var text = $$"""
                {{quote}}
                {
                    "name": "mehran",
                    "desc": "software"
                }
                {{quote}}
                """;

            var json = OpenAiUtil.ParseJson<JsonObject>(text);

            Assert.NotNull(json);
        }

        [Fact]
        public void ParseJson_JsonPrefix()
        {
            var text = """
                ```json
                {
                    "name": "mehran",
                    "desc": "software"
                }
                ```
                """;

            var json = OpenAiUtil.ParseJson<JsonObject>(text);

            Assert.NotNull(json);
        }

        [Fact]
        public void ParseJson_DoubleQuote()
        {
            var text = """
                "
                {
                    "name": "mehran",
                    "desc": "software"
                }
                ```
                """;

            var json = OpenAiUtil.ParseJson<JsonObject>(text);

            Assert.NotNull(json);
        }

    }
}
