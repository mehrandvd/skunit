using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using skUnit.Exceptions;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers.Assertions;
using System.Linq;

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
            

            if (chatItem.Role == AuthorRole.System)
            {
                chatHistory.AddSystemMessage(chatItem.Content);
                Log($"## [{chatItem.Role.ToString().ToUpper()}]");
                Log(chatItem.Content);
                Log();
            } 
            else if (chatItem.Role == AuthorRole.User)
            {
                chatHistory.AddUserMessage(chatItem.Content);
                Log($"## [{chatItem.Role.ToString().ToUpper()}]");
                Log(chatItem.Content);
                Log();
            } 
            else if (chatItem.Role == AuthorRole.Assistant)
            {
                Log($"## [EXPECTED ANSWER]");
                Log(chatItem.Content);
                Log();

                getAnswerFunc ??= async (history) =>
                {
                    var chatService = kernel.GetRequiredService<IChatCompletionService>();
                    var result = await chatService.GetChatMessageContentsAsync(history);

                    return result.First().Content ?? "";
                };

                var answer = await getAnswerFunc(chatHistory);

                // To let chatHistory stay clean for getting the answer
                chatHistory.AddAssistantMessage(chatItem.Content);


                Log($"### [ACTUAL ANSWER]");
                Log(answer);
                Log();

                foreach (var assertion in chatItem.Assertions)
                {
                    await CheckAssertionAsync(assertion, answer);
                }
            }

            foreach (var functionCall in chatItem.FunctionCalls)
            {
                Log($"## CALL {functionCall.FunctionName}");
                Log(functionCall.ArgumentsText);
                Log();

                var function = kernel.Plugins[functionCall.PluginName][functionCall.FunctionName];

                var arguments = new KernelArguments();


                var lastUserIndex = chatHistory.IndexOf(chatHistory.Last(c => c.Role == AuthorRole.User));

                var history = new ChatHistory(chatHistory.Take(chatHistory.Count - lastUserIndex - 2));
                var historyText = string.Join(
                    Environment.NewLine,
                    history.Select(c => $"[{c.Role}]: {c.Content}"));

                var input = chatHistory.ElementAt(lastUserIndex).Content;

                arguments.Add("input", input);
                arguments.Add("history", historyText);

                foreach (var functionCallArgument in functionCall.Arguments)
                {

                    if (functionCallArgument.LiteralValue is not null)
                    {
                        arguments.Add(functionCallArgument.Name, functionCallArgument.LiteralValue);
                    }
                    else if (functionCallArgument.InputVariable is not null)
                    {
                        var fullChat = string.Join(
                            Environment.NewLine,
                            chatHistory.Select(c => $"[{c.Role}]: {c.Content}"));

                        var value = functionCallArgument.InputVariable switch
                        {
                            "input" => input,
                            "history" => historyText,
                            "chat" => fullChat,
                            _ => throw new InvalidOperationException($"Unknown parameter: {functionCallArgument.InputVariable} in calling {functionCall.FunctionName}")
                        };

                        arguments.Add(functionCallArgument.Name, value);
                    }
                    else
                    {
                        throw new InvalidOperationException($"""
                                    Invalid function arguments:
                                    {functionCallArgument} 
                                    """);
                    }
                }

                foreach (var argument in arguments)
                {
                    Log($"### ARGUMENT {argument.Key}");
                    Log(argument.Value?.ToString());
                    Log();
                }

                var result = await function.InvokeAsync<string>(kernel, arguments);

                Log($"## CALL RESULT {functionCall.FunctionName}");
                Log(result);
                Log();

                foreach (var assertion in functionCall.Assertions)
                {
                    await CheckAssertionAsync(assertion, result ?? "");
                }
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

    /// <summary>
    /// Checks whether the <paramref name="scenario"/> passes on the given plugin URL
    /// using its ChatCompletionService.
    /// </summary>
    /// <param name="pluginUrl"></param>
    /// <param name="scenario"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
    public async Task CheckPluginByUrlAsync(string pluginUrl, ChatScenario scenario)
    {
        var kernel = new KernelBuilder().WithOpenAIChatCompletionService(pluginUrl).Build();
        await CheckChatScenarioAsync(kernel, scenario);
    }

    /// <summary>
    /// Checks whether all of the <paramref name="scenarios"/> passes on the given plugin URL
    /// using its ChatCompletionService.
    /// </summary>
    /// <param name="pluginUrl"></param>
    /// <param name="scenarios"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
    public async Task CheckPluginByUrlAsync(string pluginUrl, List<ChatScenario> scenarios)
    {
        var kernel = new KernelBuilder().WithOpenAIChatCompletionService(pluginUrl).Build();
        await CheckChatScenarioAsync(kernel, scenarios);
    }
}
