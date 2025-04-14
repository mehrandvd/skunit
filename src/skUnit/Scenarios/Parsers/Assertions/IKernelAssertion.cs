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
        /// Checks if the <paramref name="response"/> can pass the assertion using <paramref name="semantic"/>
        /// </summary>
        /// <param name="semantic"></param>
        /// <param name="response"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        Task Assert(Semantic semantic, ChatResponse response, IEnumerable<object>? history = null);
        string AssertionType { get; }
        string Description { get; }
    }
}
