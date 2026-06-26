using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using skUnit.Exceptions;
using skUnit.Runners;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit;

public class ChatScenarioRunner
{
    private readonly ILogger _logger;
    private readonly SemanticEvaluator _semanticEvaluator;

    public ChatScenarioRunner(IChatClient assertionClient, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(assertionClient);
        _semanticEvaluator = new SemanticEvaluator(assertionClient);
        _logger = logger ?? NullLogger.Instance;
    }

    public ChatScenarioRunner(IChatClient assertionClient, Action<string> onLog)
    {
        ArgumentNullException.ThrowIfNull(assertionClient);
        ArgumentNullException.ThrowIfNull(onLog);
        _semanticEvaluator = new SemanticEvaluator(assertionClient);
        _logger = new DelegateLoggerAdapter(onLog);
    }

    public Task RunAsync(
        ChatScenario scenario,
        IChatClient chatClient,
        IReadOnlyList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        return RunScenarioAsync(
            scenario,
            CreateResponseProvider(chatClient: chatClient),
            initialMessages,
            options,
            cancellationToken);
    }

    public Task RunAsync(
        ChatScenario scenario,
        AIAgent agent,
        IReadOnlyList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agent);
        return RunScenarioAsync(
            scenario,
            CreateResponseProvider(agent: agent),
            initialMessages,
            options,
            cancellationToken);
    }

    public Task RunAsync(
        ChatScenario scenario,
        Func<IReadOnlyList<ChatMessage>, CancellationToken, ValueTask<ChatResponse>> getResponseAsync,
        IReadOnlyList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(getResponseAsync);
        return RunScenarioAsync(scenario, getResponseAsync, initialMessages, options, cancellationToken);
    }

    public async Task RunAsync(
        IEnumerable<ChatScenario> scenarios,
        IChatClient chatClient,
        IReadOnlyList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        ArgumentNullException.ThrowIfNull(scenarios);

        foreach (var scenario in scenarios)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await RunAsync(scenario, chatClient, initialMessages, options, cancellationToken);
            Log();
            Log("----------------------------------");
            Log();
        }
    }

    public async Task RunAsync(
        IEnumerable<ChatScenario> scenarios,
        AIAgent agent,
        IReadOnlyList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agent);
        ArgumentNullException.ThrowIfNull(scenarios);

        foreach (var scenario in scenarios)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await RunAsync(scenario, agent, initialMessages, options, cancellationToken);
            Log();
            Log("----------------------------------");
            Log();
        }
    }

    public async Task RunAsync(
        IEnumerable<ChatScenario> scenarios,
        Func<IReadOnlyList<ChatMessage>, CancellationToken, ValueTask<ChatResponse>> getResponseAsync,
        IReadOnlyList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(getResponseAsync);
        ArgumentNullException.ThrowIfNull(scenarios);

        foreach (var scenario in scenarios)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await RunAsync(scenario, getResponseAsync, initialMessages, options, cancellationToken);
            Log();
            Log("----------------------------------");
            Log();
        }
    }

    private async Task RunScenarioAsync(
        ChatScenario scenario,
        Func<IReadOnlyList<ChatMessage>, CancellationToken, ValueTask<ChatResponse>> getResponseAsync,
        IReadOnlyList<ChatMessage>? initialMessages,
        ScenarioRunOptions? options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(getResponseAsync);

        options ??= new ScenarioRunOptions();
        var startHistory = initialMessages ?? [];

        var scenarioDocument = scenario.ToDocument();
        Log($"# SCENARIO {scenarioDocument.Description}");
        Log();

        var exceptions = new List<Exception>();
        for (var round = 0; round < options.TotalRuns; round++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var roundChatHistory = new List<ChatMessage>(startHistory);

            if (options.TotalRuns > 1)
            {
                Log($"# ROUND {round + 1}");
                Log();
            }

            try
            {
                await RunCoreAsync(scenarioDocument, getResponseAsync, roundChatHistory, cancellationToken);
            }
            catch (Exception ex)
            {
                Log($"❌ Exception: {ex.Message}");
                exceptions.Add(ex);
            }
        }

        var requiredSuccessRuns = options.RequiredSuccessRuns ?? options.TotalRuns;
        if (options.RequiredSuccessRuns is not null &&
            (options.RequiredSuccessRuns < 1 || options.RequiredSuccessRuns > options.TotalRuns))
        {
            throw new ArgumentOutOfRangeException(
                nameof(options.RequiredSuccessRuns),
                options.RequiredSuccessRuns,
                $"Must be between 1 and {options.TotalRuns} when specified.");
        }

        var successfulRuns = options.TotalRuns - exceptions.Count;
        if (successfulRuns < requiredSuccessRuns)
        {
            var message =
                $"Only {successfulRuns} of {options.TotalRuns} runs passed, which is below the required success runs of {requiredSuccessRuns}.";
            Log(message);
            throw new SemanticAssertException(message);
        }
    }

    private async Task RunCoreAsync(
        ScenarioDocument scenario,
        Func<IReadOnlyList<ChatMessage>, CancellationToken, ValueTask<ChatResponse>> getResponseAsync,
        List<ChatMessage> chatHistory,
        CancellationToken cancellationToken)
    {
        var queue = new Queue<ScenarioStep>(scenario.Steps);
        while (queue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var chatItem = queue.Dequeue();

            if (chatItem.Role == ChatRole.System || chatItem.Role == ChatRole.User || chatItem.Role == ChatRole.Tool)
            {
                chatHistory.Add(chatItem.ToChatMessage());
                Log($"## [{chatItem.Role.ToString().ToUpperInvariant()}]");
                Log(chatItem.Text);
                Log();
                continue;
            }

            if (chatItem.Role != ChatRole.Assistant)
            {
                continue;
            }

            Log("## [EXPECTED ANSWER]");
            Log(chatItem.Text);
            Log();

            var response = await getResponseAsync(chatHistory, cancellationToken);
            chatHistory.Add(chatItem.ToChatMessage());

            Log("### [ACTUAL ANSWER]");
            Log(response.Text);
            Log();

            foreach (var assertion in chatItem.Assertions)
            {
                await CheckAssertionAsync(assertion, response, chatHistory, cancellationToken);
            }
        }
    }

    private async Task CheckAssertionAsync(
        IChatAssertion assertion,
        ChatResponse response,
        IReadOnlyList<ChatMessage> chatHistory,
        CancellationToken cancellationToken)
    {
        Log($"### ASSERT {assertion.AssertionType}");
        Log(assertion.Description);

        try
        {
            await assertion.Assert(_semanticEvaluator, response, chatHistory, cancellationToken);
            Log("✅ OK");
            Log();
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

    private static Func<IReadOnlyList<ChatMessage>, CancellationToken, ValueTask<ChatResponse>> CreateResponseProvider(
        IChatClient? chatClient = null,
        AIAgent? agent = null)
    {
        if (chatClient is not null)
        {
            return (history, cancellationToken) => new ValueTask<ChatResponse>(
                chatClient.GetResponseAsync(history, cancellationToken: cancellationToken));
        }

        if (agent is not null)
        {
            return async (history, cancellationToken) =>
            {
                var result = await agent.RunAsync(history, cancellationToken: cancellationToken);
                return new ChatResponse(new ChatMessage(ChatRole.Assistant, result.Text));
            };
        }

        throw new InvalidOperationException("A response provider source must be specified.");
    }

    private void Log(string? message = "")
    {
        _logger.LogInformation("{Message}", message ?? "");
    }
}
