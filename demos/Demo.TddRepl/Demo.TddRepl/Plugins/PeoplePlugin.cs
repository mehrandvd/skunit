using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.TddRepl.Plugins
{
    public class PeoplePlugin
    {
        [KernelFunction]
        [Description("Returns some information for given person.")]
        public string GetPersonInfo(
            string person,
            [Description("User attitude: angry|normal")]
            string userAttitude)
        {
            if (userAttitude == "angry")
            {
                return $"{person} is a therapist.";
            }

            return $"{person} is a software architect.";
        }
    }
}
