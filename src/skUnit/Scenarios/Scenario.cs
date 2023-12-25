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
        public static List<TScenario> LoadFromText(string text, string config)
        {
            var parser = new TScenarioParser();
            var scenario = parser.Parse(text, config);
            return scenario;
        }
    }
}
