using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skUnit.Exceptions
{
    /// <summary>
    /// Throws when a semantic assertion is failed.
    /// </summary>
    public class SemanticAssertException : Exception
    {
        public SemanticAssertException(string message) : base(message)
        {

        }
    }
}
