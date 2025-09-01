using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit.Scenarios.Parsers
{
    public class KernelAssertionParser
    {
        /// <summary>
        /// Parses an assertion text to a related KernelAssertion. For example:
        /// <code>
        /// HasConditionAssertion, AreSimilarAssertion, ContainsAllAssertion
        /// </code>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IKernelAssertion Parse(string text, string type)
        {
            return type.Trim().ToLower() switch
            {
                "semanticcondition" or "semantic-condition" or "condition"
                    => new HasConditionAssertion() { Condition = text },
                "semanticsimilar" or "semantic-similar" or "similar"
                    => new AreSimilarAssertion() { ExpectedAnswer = text },
                "contains" or "contain" or "containsall" or "containstext"
                    => new ContainsAllAssertion() { Texts = text.Split(',', '،') },
                "containsany" or "containsanyof"
                    => new ContainsAnyAssertion() { Texts = text.Split(',', '،') },
                "equal" or "equals" or "exactmatch"
                    => new EqualsAssertion() { ExpectedAnswer = text },
                "jsoncheck" or "jsonstructure" or "json"
                    => new JsonCheckAssertion().SetJsonAssertText(text),
                "functioncall" or "functioninvocation" or "toolcall"
                    => new FunctionCallAssertion().SetJsonAssertText(text),
                "empty" or "isempty"
                    => new EmptyAssertion(),
                "notempty" or "notEmpty" or "hasvalue"
                    => new NotEmptyAssertion(),
                "isanyof" or "oneOf" or "anyof"
                    => new IsAnyOfAssertion() { Texts = text.Split(',', '،') },

                _ => throw new InvalidOperationException($"Not valid assert type: {type}")
            };
        }
    }
}
