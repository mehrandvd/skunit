﻿using skUnit.Scenarios.Parsers.Assertions;
using skUnit.Scenarios.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using skUnit.Scenarios;

namespace skUnit.Tests.ScenarioTests;

public class ParseChatScenarioTests
{
    [Fact]
    public void ParseScenario_Complex_MustWork()
    {
        var scenarioText = """
                # SCENARIO Height Discussion

                ### [USER]
                Is Eiffel tall?

                ## [AGENT]
                Yes it is

                ### CHECK SemanticCondition
                Approves that eiffel tower is tall or is positive about it.

                ## [USER]
                What about everest mountain?

                ## [AGENT]
                Yes it is tall too

                ### CHECK SemanticCondition
                The sentence is positive.

                ## [USER]
                What about a mouse?

                ## [AGENT]
                No it is not tall.

                ### CHECK SemanticCondition
                The sentence is negative.
                
                """;

        var scenarios = ChatScenario.LoadFromText(scenarioText, "");

        Assert.NotEmpty(scenarios);

        var first = scenarios.First();

        Assert.Equal(6, first.ChatItems.Count);
    }

    [Fact]
    public void ParseScenario_SpecialId_MustWork()
    {
        var scenarioText = """
                # sk SCENARIO Height Discussion

                ### sk [USER]
                Is Eiffel tall?

                ## sk [AGENT]
                Yes it is

                ### sk CHECK SemanticCondition
                Approves that eiffel tower is tall or is positive about it.

                ## sk [USER]
                What about everest mountain?

                ## sk [AGENT]
                Yes it is tall too

                ### sk CHECK SemanticCondition
                The sentence is positive.

                ## sk [USER]
                What about a mouse?

                ## sk [AGENT]
                No it is not tall.

                ### sk CHECK SemanticCondition
                The sentence is negative.
                
                """;

        var scenarios = ChatScenario.LoadFromText(scenarioText, "");

        Assert.NotEmpty(scenarios);

        var first = scenarios.First();

        Assert.Equal(6, first.ChatItems.Count);
    }
}