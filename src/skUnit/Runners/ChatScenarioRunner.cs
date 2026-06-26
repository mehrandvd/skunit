using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using skUnit.Exceptions;
using skUnit.Runners;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers.Assertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using FunctionCallContent = Microsoft.Extensions.AI.FunctionCallContent;
using FunctionResultContent = Microsoft.Extensions.AI.FunctionResultContent;

namespace skUnit
{
    public class ChatScenarioRunner
    {
        private readonly ILogger _logger;
        private SemanticAgent Semantic { get; set; }

        /// <summary>
        /// Gets or sets the default logger for all instances of ChatScenarioRunner that do not have a specific logger provided.
        /// </summary>
        private static ILogger? DefaultLogger { get; set; }
        
        /// <summary>
        /// Creates a new ChatScenarioRunner with an assertion client and logger.
        /// </summary>
        /// <param name="assertionClient">The chat client used for semantic evaluations and assertions (not the system under test)</param>
        /// <param name="logger">Optional logger for test execution output</param>
        public ChatScenarioRunner(IChatClient assertionClient, ILogger<ChatScenarioRunner>? logger = null)
        {
            Semantic = new SemanticAgent(assertionClient);
            _logger = logger ?? DefaultLogger ?? NullLogger<ChatScenarioRunner>.Instance;
        }

        /// <summary>
        /// Creates a new ChatScenarioRunner with an assertion agent and logger.
        /// </summary>
        /// <param name="assertionAgent">The AI agent used for semantic evaluations and assertions (not the system under test)</param>
        /// <param name="logger">Optional logger for test execution output</param>
        public ChatScenarioRunner(AIAgent assertionAgent, ILogger<ChatScenarioRunner>? logger = null)
        {
            Semantic = new SemanticAgent(assertionAgent);
            _logger = logger ?? NullLogger<ChatScenarioRunner>.Instance;
        }

        /// <summary>
        /// Creates a new ChatScenarioRunner with an assertion client and action-based logging.
        /// </summary>
        /// <param name="assertionClient">The chat client used for semantic evaluations and assertions (not the system under test)</param>
        /// <param name="onLog">Optional action for logging test execution output</param>
        public ChatScenarioRunner(IChatClient assertionClient, Action<string>? onLog)
        {
            Semantic = new SemanticAgent(assertionClient);
            _logger = onLog != null ? new DelegateLoggerAdapter<ChatScenarioRunner>(onLog) : NullLogger<ChatScenarioRunner>.Instance;
        }

        /// <summary>
        /// Creates a new ChatScenarioRunner with an assertion agent and action-based logging.
        /// </summary>
        /// <param name="assertionAgent">The AI agent used for semantic evaluations and assertions (not the system under test)</param>
        /// <param name="onLog">Optional action for logging test execution output</param>
        public ChatScenarioRunner(AIAgent assertionAgent, Action<string>? onLog)
        {
            Semantic = new SemanticAgent(assertionAgent);
            _logger = onLog != null ? new DelegateLoggerAdapter<ChatScenarioRunner>(onLog) : NullLogger<ChatScenarioRunner>.Instance;
        }

        /// <summary>
        /// Sets the default logger for all instances of ChatScenarioRunner that do not have a specific logger provided.
        /// </summary>
        /// <param name="logger">The logger to set as the default</param>
        public static void SetDefaultLogger(ILogger logger)
        {
            DefaultLogger = logger;
        }

        /// <summary>
        /// Sets the default logger for all instances of ChatScenarioRunner that do not have a specific logger provided.
        /// </summary>
        /// <param name="onLog">The action to set as the default logger</param>
        public void SetDefaultLogger(Action<string> onLog)
        {
            DefaultLogger = new DelegateLoggerAdapter<ChatScenarioRunner>(onLog);
        }

        private void Log(string? message = "")
        {
            LoggerExtensions.LogInformation(_logger, "{Message}", message ?? "");
        }

        private void LogWarning(string message)
        {
            LoggerExtensions.LogWarning(_logger, "{Message}", message);
        }

        private async Task CheckAssertionAsync(IChatAssertion assertion, ChatResponse response, IList<ChatMessage> chatHistory, string keyword = "ASSERT")
        {
            Log($"### {keyword} {assertion.AssertionType}");
            Log($"{assertion.Description}");

            try
            {
                await assertion.Assert(Semantic, response, chatHistory);
                Log($"✅ OK");
                Log("");
            }
            catch (SemanticAssertException exception)
            {
                Log("❌ FAIL");
                Log("Reason:");
                Log(exception.Message);
                Log();
                throw;
            }
        }

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
        /// <param name="initialMessages"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the AI client was unable to generate a valid response.</exception>
        public async Task RunAsync(
            ChatScenario scenario,
            IChatClient chatClient,
            IList<ChatMessage>? initialMessages = null,
            ScenarioRunOptions? options = null
        )
        {
            ArgumentNullException.ThrowIfNull(chatClient);
            await RunScenarioAsync(scenario, CreatePopulateAnswer(chatClient, null, null), initialMessages, options);
        }

        /// <summary>
        /// Runs the <paramref name="scenario"/> against the given <paramref name="agent"/>
        /// using its agent execution flow.
        /// </summary>
        /// <param name="scenario">The chat scenario to execute.</param>
        /// <param name="agent">The AI agent to test.</param>
        /// <param name="initialMessages">Optional initial chat history that should be present before the scenario starts.</param>
        /// <param name="options">Optional configuration for scenario execution.</param>
        /// <returns></returns>
        public async Task RunAsync(
            ChatScenario scenario,
            AIAgent agent,
            IList<ChatMessage>? initialMessages = null,
            ScenarioRunOptions? options = null
        )
        {
            ArgumentNullException.ThrowIfNull(agent);
            await RunScenarioAsync(scenario, CreatePopulateAnswer(null, agent, null), initialMessages, options);
        }

        /// <summary>
        /// Runs the <paramref name="scenario"/> using the supplied custom response function.
        /// </summary>
        /// <param name="scenario">The chat scenario to execute.</param>
        /// <param name="getAnswerFunc">The custom function that generates responses from chat history.</param>
        /// <param name="initialMessages">Optional initial chat history that should be present before the scenario starts.</param>
        /// <param name="options">Optional configuration for scenario execution.</param>
        /// <returns>A task that completes when the scenario execution finishes.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="getAnswerFunc"/> is null.</exception>
        /// <exception cref="SemanticAssertException">If the scenario assertions fail.</exception>
        public async Task RunAsync(
            ChatScenario scenario,
            Func<IList<ChatMessage>, Task<ChatResponse>> getAnswerFunc,
            IList<ChatMessage>? initialMessages = null,
            ScenarioRunOptions? options = null
        )
        {
            ArgumentNullException.ThrowIfNull(getAnswerFunc);
            await RunScenarioAsync(scenario, getAnswerFunc, initialMessages, options);
        }

        private async Task RunScenarioAsync(ChatScenario scenario, Func<IList<ChatMessage>, Task<ChatResponse>> populateAnswer, IList<ChatMessage>? initialMessages, ScenarioRunOptions? options)
        {
            options ??= new ScenarioRunOptions();

            initialMessages ??= [];

            Log($"# SCENARIO {scenario.Description}");
            Log("");

            List<Exception> exceptions = [];

            for (var round = 0; round < options.TotalRuns; round++)
            {
                var roundChatHistory = new List<ChatMessage>(initialMessages);
                if (options.TotalRuns > 1)
                {
                    Log($"# ROUND {round + 1}");
                    Log();
                }

                try
                {
                    await RunCoreAsync(scenario, populateAnswer, roundChatHistory);
                }
                catch (Exception ex)
                {
                    Log($"❌ Exception: {ex.Message}");
                    exceptions.Add(ex);
                }
            }

            var successRate = 1 - (exceptions.Count * 1f / options.TotalRuns);

            if (successRate < options.MinSuccessRate)
            {
                var message = $"Only {(successRate * 100):F2}% of rounds passed, which is below the required success rate of {(options.MinSuccessRate * 100):F2}%";
                Log(message);
                throw new SemanticAssertException(message);
            }
        }

        private static Func<IList<ChatMessage>, Task<ChatResponse>> CreatePopulateAnswer(IChatClient? chatClient, AIAgent? agent, Func<IList<ChatMessage>, Task<ChatResponse>>? getAnswerFunc)
        {
            if (getAnswerFunc != null)
            {
                return getAnswerFunc;
            }

            if (chatClient != null)
            {
                return history => chatClient.GetResponseAsync(history);
            }

            if (agent != null)
            {
                return async history =>
                {
                    var result = await agent.RunAsync(history);
                    return new ChatResponse(new ChatMessage(ChatRole.Assistant, result.Text));
                };
            }

            Debug.Assert(false, "At least one of chatClient, agent, or getAnswerFunc must be specified.");
            return _ => Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, string.Empty)));
        }

        private async Task RunCoreAsync(ChatScenario scenario, Func<IList<ChatMessage>, Task<ChatResponse>> populateAnswer, IList<ChatMessage> chatHistory)
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
        /// using its chat completion functionality.
        /// </summary>
        /// <param name="scenarios">The list of chat scenarios to execute.</param>
        /// <param name="chatClient">The chat client to test.</param>
        /// <param name="initialMessages">Optional initial chat history that should be present before the scenarios start.</param>
        /// <param name="options">Optional configuration for scenario execution.</param>
        /// <returns>A task that completes when all scenarios have been executed.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="chatClient"/> is null.</exception>
        /// <exception cref="SemanticAssertException">If any scenario assertion fails.</exception>
        /// <remarks>
        /// Use the custom-function overload when you want to generate responses from chat history with your own logic.
        /// </remarks>
        public async Task RunAsync(IEnumerable<ChatScenario> scenarios, IChatClient chatClient, IList<ChatMessage>? initialMessages = null, ScenarioRunOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(chatClient);
            ArgumentNullException.ThrowIfNull(scenarios);
            foreach (var scenario in scenarios)
            {
                await RunAsync(scenario, chatClient, initialMessages, options);
                Log();
                Log("----------------------------------");
                Log();
            }
        }

        /// <summary>
        /// Runs all of the <paramref name="scenarios"/> using the supplied custom response function.
        /// </summary>
        /// <param name="scenarios">The list of chat scenarios to execute.</param>
        /// <param name="getAnswerFunc">The custom function that generates responses from chat history.</param>
        /// <param name="initialMessages">Optional initial chat history that should be present before the scenarios start.</param>
        /// <param name="options">Optional configuration for scenario execution.</param>
        /// <returns>A task that completes when all scenarios have been executed.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="getAnswerFunc"/> is null.</exception>
        /// <exception cref="SemanticAssertException">If any scenario assertion fails.</exception>
        /// <remarks>
        /// Use this overload when you need to generate answers from chat history with your own logic instead of a chat client or agent.
        /// </remarks>
        public async Task RunAsync(IEnumerable<ChatScenario> scenarios, Func<IList<ChatMessage>, Task<ChatResponse>> getAnswerFunc, IList<ChatMessage>? initialMessages = null, ScenarioRunOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(getAnswerFunc);
            ArgumentNullException.ThrowIfNull(scenarios);
            foreach (var scenario in scenarios)
            {
                await RunAsync(scenario, getAnswerFunc, initialMessages, options);
                Log();
                Log("----------------------------------");
                Log();
            }
        }

        /// <summary>
        /// Runs all of the <paramref name="scenarios"/> against the given <paramref name="agent"/>.
        /// </summary>
        /// <param name="scenarios">The list of chat scenarios to execute.</param>
        /// <param name="agent">The AI agent to test.</param>
        /// <param name="initialMessages">Optional initial chat history that should be present before the scenarios start.</param>
        /// <param name="options">Optional configuration for scenario execution.</param>
        /// <returns>A task that completes when all scenarios have been executed.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="agent"/> is null.</exception>
        /// <exception cref="SemanticAssertException">If any scenario assertion fails.</exception>
        public async Task RunAsync(IEnumerable<ChatScenario> scenarios, AIAgent agent, IList<ChatMessage>? initialMessages = null, ScenarioRunOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(agent);
            ArgumentNullException.ThrowIfNull(scenarios);
            foreach (var scenario in scenarios)
            {
                await RunAsync(scenario, agent, initialMessages, options);
                Log();
                Log("----------------------------------");
                Log();
            }
        }
    }
}