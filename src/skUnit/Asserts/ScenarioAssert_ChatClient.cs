using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticValidation;
using skUnit.Exceptions;
using skUnit.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace skUnit
{
    public partial class ScenarioAssert
    {
        /// <summary>
        /// Checks whether the <paramref name="scenario"/> passes on the given <paramref name="chatClient"/>
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
        /// <param name="chatClient"></param>
        /// <param name="getAnswerFunc"></param>
        /// <param name="chatHistory"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
        public async Task PassAsync(
            ChatScenario scenario,
            IChatClient? chatClient = null, 
            Func<IList<ChatMessage>, Task<string>>? getAnswerFunc = null,
            IList<ChatMessage>? chatHistory = null

            )
        {
            if (chatClient is null && getAnswerFunc is null)
                throw new InvalidOperationException("Both chatClient and getAnswerFunc can not be null. One of them should be specified");

            chatHistory ??= [];

            Log($"# SCENARIO {scenario.Description}");
            Log("");

            var queue = new Queue<ChatItem>(scenario.ChatItems);

            while (queue.Count > 0)
            {
                var chatItem = queue.Dequeue();


                if (chatItem.Role == AuthorRole.System)
                {
                    chatHistory.Add(new ChatMessage(ChatRole.System, chatItem.Content));
                    Log($"## [{chatItem.Role.ToString().ToUpper()}]");
                    Log(chatItem.Content);
                    Log();
                }
                else if (chatItem.Role == AuthorRole.User)
                {
                    chatHistory.Add(new ChatMessage(ChatRole.User, chatItem.Content));
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
                        if (chatClient is null)
                            throw new InvalidOperationException("Both chatClient and getAnswerFunc can not be null. One of them should be specified");

                        var result = await chatClient.CompleteAsync(history);

                        return result.Message.Text ?? "";
                    };

                    var answer = await getAnswerFunc(chatHistory);

                    // To let chatHistory stay clean for getting the answer
                    chatHistory.Add(new ChatMessage(ChatRole.Assistant, chatItem.Content));


                    Log($"### [ACTUAL ANSWER]");
                    Log(answer);
                    Log();

                    foreach (var assertion in chatItem.Assertions)
                    {
                        await CheckAssertionAsync(assertion, answer);
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
        public async Task PassAsync(List<ChatScenario> scenarios, IChatClient? chatClient = null, Func<IList<ChatMessage>, Task<string>>? getAnswerFunc = null, IList<ChatMessage>? chatHistory = null)
        {
            foreach (var scenario in scenarios)
            {
                await PassAsync(scenario, chatClient, getAnswerFunc, chatHistory);
                Log();
                Log("----------------------------------");
                Log();
            }
        }



    }
}
