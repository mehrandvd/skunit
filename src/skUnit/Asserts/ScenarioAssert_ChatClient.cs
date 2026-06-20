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
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
        [Obsolete("Use ChatScenarioRunner.RunAsync instead. This method will be removed in a future version.", false)]
        public async Task PassAsync(
            ChatScenario scenario,
            IChatClient? chatClient = null,
            Func<IList<ChatMessage>, Task<ChatResponse>>? getAnswerFunc = null,
            IList<ChatMessage>? chatHistory = null,
            ScenarioRunOptions? options = null
            )
        {
            await _runner.RunAsync(scenario, chatClient, getAnswerFunc, chatHistory, options);
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
        /// <param name="chatClient"></param>
        /// <param name="getAnswerFunc"></param>
        [Experimental("SKEXP0001")]
        [Obsolete("Use ChatScenarioRunner.RunAsync instead. This method will be removed in a future version.", false)]
        public async Task PassAsync(ChatScenario scenario, Kernel kernel, IList<ChatMessage>? chatHistory = null, ScenarioRunOptions? options = null)
        {
#pragma warning disable SKEXP0001
            await _runner.RunAsync(scenario, kernel, chatHistory, options);
#pragma warning restore SKEXP0001
        }
    }
}
