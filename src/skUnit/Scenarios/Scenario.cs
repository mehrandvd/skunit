﻿using System;
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
        public static TScenario LoadFromText(string text)
        {
            var parser = new TScenarioParser();
            var scenario = parser.Parse(text);
            return scenario;
        }

        public static async Task<TScenario> LoadFromResourceAsync(string resource, Assembly assembly)
        {
            await using Stream? stream = assembly.GetManifestResourceStream(resource);

            if (stream is null)
                throw new InvalidOperationException($"Resource not found '{resource}'");

            using StreamReader reader = new StreamReader(stream);
            var result = await reader.ReadToEndAsync();
            var parser = new TScenarioParser();
            var scenario = parser.Parse(result);
            return scenario;
        }
    }
}
