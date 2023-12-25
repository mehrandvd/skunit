﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig.Helpers;
using SemanticValidation;

namespace skUnit.Scenarios.Parsers.Assertions
{
    /// <summary>
    /// An assertion that can be applied to the answer returned by a kernel.
    /// </summary>
    public interface IKernelAssertion
    {
        /// <summary>
        /// Checks if the <paramref name="answer"/> can pass the assertion using <paramref name="semantic"/>
        /// </summary>
        /// <param name="semantic"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        Task Assert(Semantic semantic, string answer);
        string AssertionType { get; }
        string Description { get; }
    }
}
