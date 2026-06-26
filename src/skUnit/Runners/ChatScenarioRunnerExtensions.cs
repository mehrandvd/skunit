using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using skUnit.Scenarios;

namespace skUnit;

public static class ChatScenarioExecutionExtensions
{
    public static Task ExecuteScenarioAsync(
        this IChatClient chatClient,
        ChatScenario scenario,
        IChatClient? assertionClient = null,
        IReadOnlyList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        ArgumentNullException.ThrowIfNull(scenario);

        var runner = new ChatScenarioRunner(assertionClient ?? chatClient);
        return runner.RunAsync(scenario, chatClient, initialMessages, options, cancellationToken);
    }

    public static Task ExecuteScenarioAsync(
        this IChatClient chatClient,
        IEnumerable<ChatScenario> scenarios,
        IChatClient? assertionClient = null,
        IReadOnlyList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        ArgumentNullException.ThrowIfNull(scenarios);

        var runner = new ChatScenarioRunner(assertionClient ?? chatClient);
        return runner.RunAsync(scenarios, chatClient, initialMessages, options, cancellationToken);
    }

    public static Task ExecuteScenarioAsync(
        this AIAgent agent,
        ChatScenario scenario,
        IChatClient assertionClient,
        IReadOnlyList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agent);
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(assertionClient);

        var runner = new ChatScenarioRunner(assertionClient);
        return runner.RunAsync(scenario, agent, initialMessages, options, cancellationToken);
    }

    public static Task ExecuteScenarioAsync(
        this AIAgent agent,
        IEnumerable<ChatScenario> scenarios,
        IChatClient assertionClient,
        IReadOnlyList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agent);
        ArgumentNullException.ThrowIfNull(scenarios);
        ArgumentNullException.ThrowIfNull(assertionClient);

        var runner = new ChatScenarioRunner(assertionClient);
        return runner.RunAsync(scenarios, agent, initialMessages, options, cancellationToken);
    }
}
