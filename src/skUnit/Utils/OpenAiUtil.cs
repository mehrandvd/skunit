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
        /// <summary>
        /// Parses <paramref name="text"/> to JSON in a more tolerant way suitable
        /// for OpenAI results.
        /// Sometimes even the prompt dictated to return a JSON, but OpenAI returns it
        /// with some additional characters like a heading and tailing ```, or even a starting ```json.
        /// This method tries its best to parse these texts to a JSON.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <returns></returns>
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
