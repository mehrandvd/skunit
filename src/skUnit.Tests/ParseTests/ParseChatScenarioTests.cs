using Microsoft.Extensions.AI;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit.Tests.ParseTests;

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

        var scenarios = ChatScenario.LoadFromText(scenarioText);

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

        var scenarios = ChatScenario.LoadFromText(scenarioText);

        Assert.NotEmpty(scenarios);

        var first = scenarios.First();

        Assert.Equal(6, first.ChatItems.Count);
    }

    [Fact]
    public void ParseScenario_FunctionCall_MustWork()
    {
        var scenarioText = """
                # SCENARIO Height Discussion

                ## [USER]
                Is Eiffel tall?

                ## CALL Content.GetIntent
                ```json
                {
                    "input": "$input",
                    "options": "Order,Question"
                }
                ```
                
                ### CHECK SemanticCondition
                It is order

                ### CHECK SemanticCondition
                It is food

                ## [AGENT]
                Yes it is

                ### CHECK SemanticCondition
                Approves that eiffel tower is tall or is positive about it.

                ### CHECK SemanticCondition
                Approves that eiffel tower is tall or is positive about it.

                ## CALL Content.GetIntent
                ```json
                {
                    "input": "$input",
                    "options": "Order,Question"
                }
                ```

                ### CHECK SemanticCondition
                It is order

                ### CHECK SemanticCondition
                It is food

                ### CHECK SemanticCondition
                It is Json
                
                """;

        var scenarios = ChatScenario.LoadFromText(scenarioText);

        Assert.NotEmpty(scenarios);

        var first = scenarios.First();

        Assert.Equal(2, first.ChatItems.Count);

        var userChatItem = first.ChatItems.ElementAt(0);
        Assert.Single(userChatItem.FunctionCalls);
        Assert.Equal(2, userChatItem.FunctionCalls.First().Assertions.Count);

        var agentChatItem = first.ChatItems.ElementAt(1);
        Assert.Equal(2, agentChatItem.Assertions.Count);
        Assert.Equal("Content", agentChatItem.FunctionCalls.First().PluginName);
        Assert.Equal("GetIntent", agentChatItem.FunctionCalls.First().FunctionName);
        Assert.Equal(3, agentChatItem.FunctionCalls.First().Assertions.Count);
    }

    [Fact]
    public void ParseScenario_CheckFunctionCall_MustWork()
    {
        var scenarioText = """
                           # SCENARIO Time Discussion

                           ### [USER]
                           What time is it?

                           ## [AGENT]
                           It's 10:00 AM

                           ### ASSERT FunctionCall
                           ```json
                           {
                            "function_name": "GetCurrentTime",
                           }
                           ```
                           """;

        var scenarios = ChatScenario.LoadFromText(scenarioText);

        Assert.NotEmpty(scenarios);

        var first = scenarios.First();

        Assert.Equal(2, first.ChatItems.Count);

        var lastChatItem = first.ChatItems.Last();
        Assert.Equal(
            "GetCurrentTime",
            lastChatItem.Assertions
                    .OfType<FunctionCallAssertion>()
                    .FirstOrDefault()?
                    .FunctionName
        );

    }

    [Fact]
    public void ParseScenario_MultiModal_TextAndImage_MustWork()
    {
        var scenarioText = """
                           # SCENARIO Multi-modal Test
                           
                           ## [USER]
                           ### Text
                           This image explains how skUnit parses the chat scenarios.
                           ### Image
                           ![skUnit structure](https://github.com/mehrandvd/skunit/assets/5070766/156b0831-e4f3-4e4b-b1b0-e2ec868efb5f)
                           ### Text
                           How many scenarios are there in the picture?
                           
                           ## [AGENT]
                           There are 2 scenarios in the picture
                           
                           ### CHECK SemanticSimilar
                           There are 2 scenarios in the picture
                           """;

        var scenarios = ChatScenario.LoadFromText(scenarioText);

        Assert.NotEmpty(scenarios);

        var first = scenarios.First();
        Assert.Equal(2, first.ChatItems.Count);

        var userChatItem = first.ChatItems.First();
        Assert.Equal(3, userChatItem.Contents.Count);

        // Check first text part
        var firstTextContent = userChatItem.Contents[0] as TextContent;
        Assert.NotNull(firstTextContent);
        Assert.Contains("This image explains", firstTextContent.Text);

        // Check image part
        var imageContent = userChatItem.Contents[1] as UriContent;
        Assert.NotNull(imageContent);
        Assert.Equal("https://github.com/mehrandvd/skunit/assets/5070766/156b0831-e4f3-4e4b-b1b0-e2ec868efb5f", imageContent.Uri.ToString());

        // Check second text part
        var secondTextContent = userChatItem.Contents[2] as TextContent;
        Assert.NotNull(secondTextContent);
        Assert.Contains("How many scenarios", secondTextContent.Text);

        // Check agent response (should be plain text)
        var agentChatItem = first.ChatItems[1];
        Assert.Single(agentChatItem.Contents);
        var agentTextContent = agentChatItem.Contents[0] as TextContent;
        Assert.NotNull(agentTextContent);
        Assert.Contains("There are 2 scenarios", agentTextContent.Text);
    }

    [Fact]
    public void ParseScenario_MultiModal_ImageOnly_MustWork()
    {
        var scenarioText = """
                           # SCENARIO Image Only Test
                           
                           ## [USER]
                           ### Image
                           ![test image](https://example.com/test.jpg)
                           
                           ## [AGENT]
                           I can see the image.
                           """;

        var scenarios = ChatScenario.LoadFromText(scenarioText);

        Assert.NotEmpty(scenarios);

        var first = scenarios.First();
        Assert.Equal(2, first.ChatItems.Count);

        var userChatItem = first.ChatItems.First();
        Assert.Single(userChatItem.Contents);

        var imageContent = userChatItem.Contents[0] as UriContent;
        Assert.NotNull(imageContent);
        Assert.Equal("https://example.com/test.jpg", imageContent.Uri.ToString());
    }

    [Fact]
    public void ParseScenario_BackwardCompatibility_TextOnly_MustWork()
    {
        var scenarioText = """
                           # SCENARIO Backward Compatibility Test
                           
                           ## [USER]
                           Just plain text without subsections
                           
                           ## [AGENT]
                           Plain text response
                           """;

        var scenarios = ChatScenario.LoadFromText(scenarioText);

        Assert.NotEmpty(scenarios);

        var first = scenarios.First();
        Assert.Equal(2, first.ChatItems.Count);

        var userChatItem = first.ChatItems.First();
        Assert.Single(userChatItem.Contents);

        var textContent = userChatItem.Contents[0] as TextContent;
        Assert.NotNull(textContent);
        Assert.Equal("Just plain text without subsections", textContent.Text);

        // Test backward compatibility property
        Assert.Equal("Just plain text without subsections", userChatItem.Content);
    }

    [Fact]
    public void ParseScenario_MultiModal_ToChatMessage_MustWork()
    {
        var scenarioText = """
                           # SCENARIO Multi-modal ChatMessage Test
                           
                           ## [USER]
                           ### Text
                           Look at this image:
                           ### Image
                           ![test](https://example.com/image.png)
                           ### Text
                           What do you see?
                           """;

        var scenarios = ChatScenario.LoadFromText(scenarioText);
        var first = scenarios.First();
        var userChatItem = first.ChatItems.First();

        // Test conversion to Microsoft.Extensions.AI ChatMessage
        var chatMessage = userChatItem.ToChatMessage();
        Assert.Equal(ChatRole.User, chatMessage.Role);
        Assert.Equal(3, chatMessage.Contents.Count);

        // Verify content types
        Assert.IsType<TextContent>(chatMessage.Contents[0]);
        Assert.IsType<UriContent>(chatMessage.Contents[1]);
        Assert.IsType<TextContent>(chatMessage.Contents[2]);

        var textContent1 = (TextContent)chatMessage.Contents[0];
        var uriContent = (UriContent)chatMessage.Contents[1];
        var textContent2 = (TextContent)chatMessage.Contents[2];

        Assert.Contains("Look at this image", textContent1.Text);
        Assert.Equal("https://example.com/image.png", uriContent.Uri.ToString());
        Assert.Contains("What do you see", textContent2.Text);
    }

    [Fact]
    public void ParseScenario_AssistantAlias_MustWork()
    {
        var scenarioText = """
                           # SCENARIO Assistant Alias Test
                           
                           ## [USER]
                           What is the capital of France?
                           
                           ## [ASSISTANT]
                           The capital of France is Paris.
                           
                           ### CHECK SemanticCondition
                           It mentions Paris as the capital.
                           """;

        var scenarios = ChatScenario.LoadFromText(scenarioText);

        Assert.Single(scenarios);
        var scenario = scenarios[0];
        Assert.Equal(2, scenario.ChatItems.Count);

        // Check USER item
        var userChatItem = scenario.ChatItems[0];
        Assert.Equal(ChatRole.User, userChatItem.Role);
        Assert.Equal("What is the capital of France?", userChatItem.Content);

        // Check ASSISTANT item (should be treated same as AGENT)
        var assistantChatItem = scenario.ChatItems[1];
        Assert.Equal(ChatRole.Assistant, assistantChatItem.Role);
        Assert.Equal("The capital of France is Paris.", assistantChatItem.Content);
        Assert.Single(assistantChatItem.Assertions);
    }

    [Fact]
    public void ParseScenario_MixedAgentAndAssistant_MustWork()
    {
        var scenarioText = """
                           # SCENARIO Mixed Agent and Assistant Test
                           
                           ## [USER]
                           Hello
                           
                           ## [AGENT]
                           Hi there!
                           
                           ## [USER]
                           How are you?
                           
                           ## [ASSISTANT]
                           I'm doing well, thank you!
                           """;

        var scenarios = ChatScenario.LoadFromText(scenarioText);

        Assert.Single(scenarios);
        var scenario = scenarios[0];
        Assert.Equal(4, scenario.ChatItems.Count);

        // Both AGENT and ASSISTANT should map to ChatRole.Assistant
        Assert.Equal(ChatRole.User, scenario.ChatItems[0].Role);
        Assert.Equal(ChatRole.Assistant, scenario.ChatItems[1].Role);
        Assert.Equal(ChatRole.User, scenario.ChatItems[2].Role);
        Assert.Equal(ChatRole.Assistant, scenario.ChatItems[3].Role);

        Assert.Equal("Hi there!", scenario.ChatItems[1].Content);
        Assert.Equal("I'm doing well, thank you!", scenario.ChatItems[3].Content);
    }

    [Fact]
    public void ParseScenario_MultipleScenarios_AgentAndAssistant_MustWork()
    {
        var scenarioText = """
                           # SCENARIO Agent Test

                           ## [USER]
                           Hello there!

                           ## [AGENT]
                           Hi, how can I help you?

                           -----

                           # SCENARIO Assistant Test

                           ## [USER]  
                           What's the weather like?

                           ## [ASSISTANT]
                           It's sunny and warm today!
                           """;

        var scenarios = ChatScenario.LoadFromText(scenarioText);

        Assert.Equal(2, scenarios.Count);

        // First scenario with [AGENT]
        var agentScenario = scenarios[0];
        Assert.Equal(2, agentScenario.ChatItems.Count);
        Assert.Equal(ChatRole.User, agentScenario.ChatItems[0].Role);
        Assert.Equal(ChatRole.Assistant, agentScenario.ChatItems[1].Role);
        Assert.Equal("Hi, how can I help you?", agentScenario.ChatItems[1].Content);

        // Second scenario with [ASSISTANT]
        var assistantScenario = scenarios[1];
        Assert.Equal(2, assistantScenario.ChatItems.Count);
        Assert.Equal(ChatRole.User, assistantScenario.ChatItems[0].Role);
        Assert.Equal(ChatRole.Assistant, assistantScenario.ChatItems[1].Role);
        Assert.Equal("It's sunny and warm today!", assistantScenario.ChatItems[1].Content);
    }
}