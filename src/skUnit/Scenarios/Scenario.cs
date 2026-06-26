using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using skUnit.Scenarios.Parsers;

namespace skUnit.Scenarios
{
    public class Scenario<TScenario, TScenarioParser> where TScenarioParser : IScenarioParser<TScenario>, new()
    {
        public static IReadOnlyList<TScenario> Parse(string markdown)
        {
            var parser = new TScenarioParser();
            var scenario = parser.Parse(markdown);
            return scenario;
        }

        public static async Task<IReadOnlyList<TScenario>> ParseFromResourceAsync(string resourceName, Assembly assembly, CancellationToken cancellationToken = default)
        {
            await using Stream? stream = assembly.GetManifestResourceStream(resourceName);

            if (stream is null)
                throw new InvalidOperationException($"Resource not found '{resourceName}'");

            using StreamReader reader = new StreamReader(stream);
            var result = await reader.ReadToEndAsync(cancellationToken);
            var parser = new TScenarioParser();
            var scenario = parser.Parse(result);
            return scenario;
        }
    }
}
