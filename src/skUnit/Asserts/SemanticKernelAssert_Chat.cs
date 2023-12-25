using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using skUnit.Exceptions;
using skUnit.Scenarios;

namespace skUnit;

public partial class SemanticKernelAssert
{
    public static async Task ScenarioChatSuccessAsync(Kernel kernel, ChatScenario scenario, Func<ChatHistory, Task<string>> getAnswerFunc)
    {
        var chatHistory = new ChatHistory();

        Log($"# TEST {scenario.Description}");
        Log("");

        var queue = new Queue<ChatItem>(scenario.ChatItems);

        while (queue.Count > 0)
        {
            var chatItem = queue.Dequeue();
            Log($"## [{chatItem.Role.ToString().ToUpper()}]");
            Log(chatItem.Content);
            Log();

            if (chatItem.Role == AuthorRole.System)
            {
                chatHistory.AddUserMessage(chatItem.Content);
                continue;
            }

            if (chatItem.Role == AuthorRole.User)
            {
                chatHistory.AddUserMessage(chatItem.Content);
            }
            else
            {
                throw new InvalidOperationException($"""
                    Expected [USER] message but it is not: 
                    [{chatItem.Role.ToString().ToUpper()}]: 
                    {chatItem.Content}
                    """);
            }

            var answer = await getAnswerFunc(chatHistory);
            Log($"## [ACTUAL ANSWER]");
            Log(answer);
            Log();

            chatItem = queue.Dequeue();
            Log($"## [EXPECTED ANSWER]");
            Log(chatItem.Content);
            Log();

            if (chatItem.Role == AuthorRole.Assistant)
            {
                chatHistory.AddUserMessage(chatItem.Content);
            }
            else
            {
                throw new InvalidOperationException($"""
                    Expected [AGENT] message but it is not: 
                    [{chatItem.Role.ToString().ToUpper()}]: 
                    {chatItem.Content}
                    """);
            }

            foreach (var assertion in chatItem.Assertions)
            {
                Log($"## CHECK {assertion.AssertionType}");
                Log($"{assertion.Description}");
                await assertion.Assert(Semantic, answer);
                Log($"OK");
                Log("");
            }

            if (chatItem.Role == AuthorRole.Assistant)
            {
                chatHistory.AddAssistantMessage(chatItem.Content);
            }
            else
            {
                throw new InvalidOperationException($"""
                    Expected [AGENT] message but it is not: 
                    [{chatItem.Role}]: {chatItem.Content}
                    """);
            }
        }
    }

    public static async Task ScenarioChatSuccessAsync(Kernel kernel, ChatScenario scenario)
    {
        Func<ChatHistory, Task<string>> getAnswerFunc = async (history) =>
        {
            var chatService = kernel.GetRequiredService<IChatCompletionService>();
            var result = await chatService.GetChatMessageContentsAsync(history);

            return result.First().Content ?? "";
        };

        await ScenarioChatSuccessAsync(kernel,  scenario, getAnswerFunc);
    }

    public static async Task ScenarioChatSuccessAsync(Kernel kernel, List<ChatScenario> scenarios)
    {
        foreach (var scenario in scenarios)
        {
            await ScenarioChatSuccessAsync(kernel, scenario);
            Log("");
            Log("----------------------------------");
            Log("");
        }
    }

    //public static async Task ScenarioChatThrowsAsync<TSemanticAssertException>(Kernel kernel, ChatScenario scenario) where TSemanticAssertException : SemanticAssertException
    //{
    //    var isThrown = false;
    //    try
    //    {
    //        var arguments = new KernelArguments();
    //        Log($"# TEST {scenario.Description}");
    //        Log("");

    //        Log($"# PROMPT");
    //        Log($"{scenario.Prompt}");
    //        Log("");

    //        foreach (var parameters in scenario.Parameters)
    //        {
    //            arguments.Add(parameters.Key, parameters.Value);
    //            Log($"## PARAMETER {parameters.Key}");
    //            Log($"{parameters.Value}");
    //            Log("");
    //        }

    //        var prompt = scenario.Prompt;
    //        if (string.IsNullOrWhiteSpace(prompt))
    //        {
    //            scenario.Parameters.TryGetValue("input", out prompt);
    //        }

    //        if (prompt is null)
    //            throw new InvalidOperationException($"""
    //                Prompt is null for scenario: 
    //                {scenario.RawText}
    //                """);

    //        var result = await kernel.InvokePromptAsync<string>(prompt, arguments);

    //        Log($"## ACTUAL ANSWER:");
    //        Log(result ?? "");
    //        Log("");

    //        foreach (var assertion in scenario.Assertions)
    //        {
    //            Log($"## ANSWER {assertion.AssertionType}");
    //            Log($"{assertion.Description}");
    //            await assertion.Assert(Semantic, result);
    //            Log($"OK");
    //            Log("");
    //        }
    //    }
    //    catch (SemanticAssertException exception)
    //    {
    //        Log("Exception as EXPECTED:");
    //        Log(exception.Message);
    //        Log("");
    //        isThrown = true;
    //    }

    //    if (!isThrown)
    //    {
    //        throw new Exception($"Expected for an exception of type: {typeof(TSemanticAssertException).FullName}");
    //    }
    //}

    //public static async Task ScenarioChatThrowsAsync<TSemanticAssertException>(Kernel kernel, List<ChatScenario> scenarios) where TSemanticAssertException : SemanticAssertException
    //{
    //    foreach (var scenario in scenarios)
    //    {
    //        await ScenarioThrowsAsync<TSemanticAssertException>(kernel, scenario);
    //        Log("");
    //        Log("----------------------------------");
    //        Log("");
    //    }
    //}
}