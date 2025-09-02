using SemanticValidation;
using skUnit.Exceptions;
using skUnit.Scenarios;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using OpenAI.Chat;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using FunctionCallContent = Microsoft.Extensions.AI.FunctionCallContent;
using FunctionResultContent = Microsoft.Extensions.AI.FunctionResultContent;
using Microsoft.SemanticKernel.ChatCompletion;

namespace skUnit
{
    public partial class ChatScenarioRunner
    {
        /// <summary>
        /// Runs the <paramref name="scenario"/> against the given <paramref name="chatClient"/>
        /// using its ChatCompletionService.
        /// If you want to test using something other than ChatCompletionService (for example using your own function),
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
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the AI client was unable to generate a valid response.</exception>
        public async Task RunAsync(
            ChatScenario scenario,
            IChatClient? chatClient = null,
            Func<IList<ChatMessage>, Task<ChatResponse>>? getAnswerFunc = null,
            IList<ChatMessage>? chatHistory = null,
            ScenarioRunOptions? options = null
            )
        {
            if (chatClient is null && getAnswerFunc is null)
                throw new InvalidOperationException("Both chatClient and getAnswerFunc can not be null. One of them should be specified");

            options ??= new ScenarioRunOptions();

            chatHistory ??= [];

            Log($"# SCENARIO {scenario.Description}");
            Log("");

            List<Exception> exceptions = [];

            for (var round = 0; round < options.TotalRuns; round++)
            {
                if (options.TotalRuns > 1)
                {
                    Log($"# ROUND {round + 1}");
                    Log();
                }

                try
                {
                    await RunCoreAsync(scenario, chatClient, getAnswerFunc, chatHistory);
                }
                catch (Exception ex)
                {
                    Log($"❌ Exception: {ex.Message}");
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count * 1f / options.TotalRuns > options.MinSuccessRate)
            {
                var message = $"Only {(options.TotalRuns - exceptions.Count) * 100 / options.TotalRuns}% of rounds passed, which is below the required success rate of {options.MinSuccessRate * 100}%";
                Log(message);
                throw new Exception(message);
            }
        }

        private async Task RunCoreAsync(ChatScenario scenario, IChatClient? chatClient, Func<IList<ChatMessage>, Task<ChatResponse>>? getAnswerFunc, IList<ChatMessage> chatHistory)
        {
            var queue = new Queue<ChatItem>(scenario.ChatItems);

            while (queue.Count > 0)
            {
                var chatItem = queue.Dequeue();

                if (chatItem.Role == ChatRole.System)
                {
                    chatHistory.Add(new ChatMessage(ChatRole.System, chatItem.Content));
                    Log($"## [{chatItem.Role.ToString().ToUpper()}]");
                    Log(chatItem.Content);
                    Log();
                }
                else if (chatItem.Role == ChatRole.User)
                {
                    chatHistory.Add(new ChatMessage(ChatRole.User, chatItem.Content));
                    Log($"## [{chatItem.Role.ToString().ToUpper()}]");
                    Log(chatItem.Content);
                    Log();
                }
                else if (chatItem.Role == ChatRole.Assistant)
                {
                    Log($"## [EXPECTED ANSWER]");
                    Log(chatItem.Content);
                    Log();

                    Func<IList<ChatMessage>, Task<ChatResponse>> populateAnswer;

                    if (getAnswerFunc != null)
                    {
                        populateAnswer = getAnswerFunc;
                    }
                    else if (chatClient != null)
                    {
                        populateAnswer = async history =>
                        {
                            var result = await chatClient.GetResponseAsync(history);
                            return result;
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Both chatClient and getAnswerFunc can not be null. One of them should be specified");
                    }

                    var response = await populateAnswer(chatHistory);

                    // To let chatHistory stay clean for getting the answer
                    chatHistory.Add(new ChatMessage(ChatRole.Assistant, chatItem.Content));

                    Log($"### [ACTUAL ANSWER]");
                    Log(response.Text);
                    Log();

                    foreach (var assertion in chatItem.Assertions)
                    {
                        await CheckAssertionAsync(assertion, response, chatHistory, "ASSERT");
                    }
                }
            }
        }

        /// <summary>
        /// Runs all of the <paramref name="scenarios"/> against the given <paramref name="chatClient"/>
        /// using its ChatCompletionService.
        /// If you want to test using something other than ChatCompletionService (for example using your own function),
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
        /// <param name="chatClient"></param>
        /// <param name="getAnswerFunc"></param>
        /// <param name="chatHistory"></param>
        [Experimental("SKEXP0001")]
        public async Task RunAsync(ChatScenario scenario, Kernel kernel, IList<ChatMessage>? chatHistory = null, ScenarioRunOptions? options = null)
        {
            var completionService = kernel.GetRequiredService<IChatCompletionService>();
            var innerClient = completionService.AsChatClient();

            ChatClientBuilder builder = new ChatClientBuilder(innerClient)
                                        .ConfigureOptions(options =>
                                        {
                                            IEnumerable<AIFunction> aiFunctions =
                                                kernel.Plugins.SelectMany(kp => kp.AsAIFunctions());
                                            options.Tools = [.. aiFunctions];
                                            //options.ToolMode = ChatToolMode.Auto;
                                        })
                                        .UseFunctionInvocation()
                                        ;

            var chatClient = builder.Build();

            await RunAsync(scenario, chatClient, null, chatHistory, options);
        }
    }
}