﻿using System.Collections;
using Microsoft.Extensions.AI;
using SemanticValidation;
using skUnit.Exceptions;

namespace skUnit.Scenarios.Parsers.Assertions;

/// <summary>
/// Checks if the answer is similar to ExpectedAnswer
/// </summary>
public class AreSimilarAssertion : IKernelAssertion
{
    /// <summary>
    /// The expected answer that the actual answer should compared with.
    /// </summary>
    public required string ExpectedAnswer { get; set; }

    /// <summary>
    /// Checks if <paramref name="answer"/> is similar to ExpectedAnswer using <paramref name="semantic"/>
    /// </summary>
    /// <param name="semantic"></param>
    /// <param name="input"></param>
    /// <param name="historytory"></param>
    /// <param name="answer"></param>
    /// <returns></returns>
    /// <exception cref="SemanticAssertException"></exception>
    public async Task Assert(Semantic semantic, string input, IEnumerable<object>? history = null)
    {
        var result = await semantic.AreSimilarAsync(input, ExpectedAnswer);

        if (!result.IsValid)
            throw new SemanticAssertException(result.Reason ?? "No reason is provided.");
    }

    public string AssertionType => "SemanticSimilar";
    public string Description => ExpectedAnswer;

    public override string ToString() => $"{AssertionType}: {ExpectedAnswer}";
}