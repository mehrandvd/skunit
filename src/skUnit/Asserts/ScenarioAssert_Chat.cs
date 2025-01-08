using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using skUnit.Exceptions;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers.Assertions;
using System.Linq;

namespace skUnit;

public partial class ScenarioAssert
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
    /// <param name="scenario"></param>
    /// <param name="kernel"></param>
    /// <param name="getAnswerFunc"></param>
    /// <param name="chatHistory"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
    public async Task PassAsync(
        ChatScenario scenario, 
        Kernel? kernel = null, Func<ChatHistory, 
        Task<string>>? getAnswerFunc = null,
        ChatHistory? chatHistory = null

        )
    {
        if (kernel is null && getAnswerFunc is null)
            throw new InvalidOperationException("Both kernel and getAnswerFunc can not be null. One of them should be specified");

        chatHistory ??= new ChatHistory();

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
                    if (kernel is null) 
                        throw new InvalidOperationException("Both kernel and getAnswerFunc can not be null. One of them should be specified");

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
                    // ToDo: After supporting IChatClient the chat history should be passed here:
                    await CheckAssertionAsync(assertion, answer, chatHistory);
                }
            }

            foreach (var functionCall in chatItem.FunctionCalls)
            {
                Log($"## CALL {functionCall.FunctionName}");
                Log(functionCall.ArgumentsText);
                Log();

                if (kernel is null)
                    throw new InvalidOperationException("kernel is not provided for function calls");

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
                    await CheckAssertionAsync(assertion, result ?? "", chatHistory);
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
    /// <param name="scenarios"></param>
    /// <param name="kernel"></param>
    /// <param name="getAnswerFunc"></param>
    /// <param name="chatHistory"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
    public async Task PassAsync(List<ChatScenario> scenarios, Kernel? kernel = null, Func<ChatHistory, Task<string>>? getAnswerFunc = null, ChatHistory? chatHistory = null)
    {
        foreach (var scenario in scenarios)
        {
            await PassAsync(scenario, kernel, getAnswerFunc, chatHistory);
            Log();
            Log("----------------------------------");
            Log();
        }
    }



}