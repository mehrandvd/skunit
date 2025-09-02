using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skUnit.Scenarios.Parsers
{
    public interface IScenarioParser<TScenario>
    {
        public abstract TScenario Parse(string text);
    }
}
