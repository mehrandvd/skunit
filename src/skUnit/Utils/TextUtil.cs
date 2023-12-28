using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace skUnit.Utils
{
    public class OpenAiUtil
    {
        public static T? ParseJson<T>(string text)
        {
            var trim =
                text.Trim(' ', '"', '\'', '`');

            var firstBrace = trim.IndexOfAny(new [] {'{', '['});
            var lastBrace = trim.LastIndexOfAny(new [] { '}', ']' })+1;

            var clear = trim[firstBrace..lastBrace];

            var json = JsonSerializer.Deserialize<T>(clear, new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
            });

            return json;
        }
    }
}
