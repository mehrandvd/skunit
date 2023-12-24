﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SemanticValidation;

namespace skUnit.Parsers.Assertions
{
    public interface IKernelAssertion
    {
        Task Assert(Semantic semantic, string answer);
        string AssertionType { get; }
    }
}
