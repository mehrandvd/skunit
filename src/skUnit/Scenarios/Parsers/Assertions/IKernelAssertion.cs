using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig.Helpers;
 using Microsoft.Extensions.AI;
using SemanticValidation;

namespace skUnit.Scenarios.Parsers.Assertions
{
    /// <summary>
    /// An assertion that can be applied to the input returned by a kernel.
    /// </summary>
    public interface IKernelAssertion
    {
        /// <summary>
        /// Checks if the <paramref name="input"/> can pass the assertion using <paramref name="semantic"/>
        /// </summary>
        /// <param name="semantic"></param>
        /// <param name="input"></param>
        /// <param name="historytory"></param>
        /// <returns></returns>
        Task Assert(Semantic semantic, string input, IEnumerable<object>? history = null);
        string AssertionType { get; }
        string Description { get; }
    }
}
