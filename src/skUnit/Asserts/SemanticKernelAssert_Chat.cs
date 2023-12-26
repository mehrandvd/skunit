using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using skUnit.Exceptions;
using skUnit.Scenarios;

namespace skUnit;

public partial class SemanticKernelAssert
{
    /// <summary>
    /// Checks whether the <paramref name="scenario"/> passes on the given <paramref name="kernel"/>
    /// using its ChatCompletionService.
    /// If you want to test the kernel using something other than ChatCompletionService (for example using your own function),
    /// pass <paramref name="getAnswerFunc"/> and specify how do you want the answer be created from chat history like:
    /// <code>
    /// getAnswerFunc = async history =>
    ///     await AnswerChatFunction.InvokeAsync(kernel, new KernelArguments()
    ///     {
    ///         ["history"] = history,
    ///     });
    /// </code>
    /// </summary>
    /// <param name="kernel"></param>
    /// <param name="scenario"></param>
    /// <param name="getAnswerFunc"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
    public async Task CheckChatScenarioAsync(Kernel kernel, ChatScenario scenario, Func<ChatHistory, Task<string>>? getAnswerFunc = null)
    {
        var chatHistory = new ChatHistory();

        Log($"# SCENARIO {scenario.Description}");
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

            getAnswerFunc ??= async (history) =>
            {
                var chatService = kernel.GetRequiredService<IChatCompletionService>();
                var result = await chatService.GetChatMessageContentsAsync(history);

                return result.First().Content ?? "";
            };

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

    /// <summary>
    /// Checks whether all of the <paramref name="scenarios"/> passes on the given <paramref name="kernel"/>
    /// using its ChatCompletionService.
    /// If you want to test the kernel using something other than ChatCompletionService (for example using your own function),
    /// pass <paramref name="getAnswerFunc"/> and specify how do you want the answer be created from chat history like:
    /// <code>
    /// getAnswerFunc = async history =>
    ///     await AnswerChatFunction.InvokeAsync(kernel, new KernelArguments()
    ///     {
    ///         ["history"] = history,
    ///     });
    /// </code>
    /// </summary>
    /// <param name="kernel"></param>
    /// <param name="scenarios"></param>
    /// <param name="getAnswerFunc"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
    public async Task CheckChatScenarioAsync(Kernel kernel, List<ChatScenario> scenarios, Func<ChatHistory, Task<string>>? getAnswerFunc = null)
    {
        foreach (var scenario in scenarios)
        {
            await CheckChatScenarioAsync(kernel, scenario, getAnswerFunc);
            Log();
            Log("----------------------------------");
            Log();
        }
    }
}