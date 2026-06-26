using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Collections.Generic;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace skUnit.Scenarios;

public static class ScenarioExtensions
{
    /// <summary>
    /// Runs a single chat scenario against the supplied chat client.
    /// </summary>
    /// <param name="chatClient">The chat client that should process the scenario.</param>
    /// <param name="scenario">The scenario to execute.</param>
    /// <param name="assertionClient">Optional chat client used for semantic assertions. Defaults to <paramref name="chatClient"/>.</param>
    /// <param name="initialMessages">Optional chat history to prepend before the scenario starts.</param>
    /// <param name="options">Optional configuration for scenario execution.</param>
    /// <returns>A task that completes when the scenario has finished running.</returns>
    public static Task RunChatScenarioAsync(
        this IChatClient chatClient,
        ChatScenario scenario,
        IChatClient? assertionClient = null,
        IList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        ArgumentNullException.ThrowIfNull(scenario);

        var runner = new ChatScenarioRunner(assertionClient: assertionClient ?? chatClient);
        return runner.RunAsync(scenario, chatClient, initialMessages, options);
    }

    /// <summary>
    /// Runs a collection of chat scenarios against the supplied chat client.
    /// </summary>
    /// <param name="chatClient">The chat client that should process the scenarios.</param>
    /// <param name="scenarios">The scenarios to execute.</param>
    /// <param name="assertionClient">Optional chat client used for semantic assertions. Defaults to <paramref name="chatClient"/>.</param>
    /// <param name="initialMessages">Optional chat history to prepend before each scenario starts.</param>
    /// <param name="options">Optional configuration for scenario execution.</param>
    /// <returns>A task that completes when all scenarios have finished running.</returns>
    public static Task RunChatScenarioAsync(
        this IChatClient chatClient,
        IEnumerable<ChatScenario> scenarios,
        IChatClient? assertionClient = null,
        IList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        ArgumentNullException.ThrowIfNull(scenarios);

        var runner = new ChatScenarioRunner(assertionClient ?? chatClient);
        return runner.RunAsync(scenarios, chatClient, initialMessages, options);
    }

    /// <summary>
    /// Runs a single chat scenario against the supplied agent.
    /// </summary>
    /// <param name="agent">The agent that should process the scenario.</param>
    /// <param name="scenario">The scenario to execute.</param>
    /// <param name="assertionClient">Optional agent used for semantic assertions. Defaults to <paramref name="agent"/>.</param>
    /// <param name="initialMessages">Optional chat history to prepend before the scenario starts.</param>
    /// <param name="options">Optional configuration for scenario execution.</param>
    /// <returns>A task that completes when the scenario has finished running.</returns>
    public static Task RunChatScenarioAsync(
        this AIAgent agent,
        ChatScenario scenario,
        IChatClient? assertionClient = null,
        IList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(agent);
        ArgumentNullException.ThrowIfNull(scenario);

        var runner = new ChatScenarioRunner(assertionClient);
        return runner.RunAsync(scenario, agent, initialMessages, options);
    }

    /// <summary>
    /// Runs a collection of chat scenarios against the supplied agent.
    /// </summary>
    /// <param name="agent">The agent that should process the scenarios.</param>
    /// <param name="scenarios">The scenarios to execute.</param>
    /// <param name="assertionClient">Optional agent used for semantic assertions. Defaults to <paramref name="agent"/>.</param>
    /// <param name="initialMessages">Optional chat history to prepend before each scenario starts.</param>
    /// <param name="options">Optional configuration for scenario execution.</param>
    /// <returns>A task that completes when all scenarios have finished running.</returns>
    public static Task RunChatScenarioAsync(
        this AIAgent agent,
        IEnumerable<ChatScenario> scenarios,
        IChatClient? assertionClient = null,
        IList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(agent);
        ArgumentNullException.ThrowIfNull(scenarios);

        var runner = new ChatScenarioRunner(assertionClient);
        return runner.RunAsync(scenarios, agent, initialMessages, options);
    }

    /// <summary>
    /// Runs the scenario against the supplied chat client.
    /// </summary>
    /// <param name="scenario">The scenario to execute.</param>
    /// <param name="chatClient">The chat client that should process the scenario.</param>
    /// <param name="assertionClient">Optional chat client used for semantic assertions. Defaults to <paramref name="chatClient"/>.</param>
    /// <param name="initialMessages">Optional chat history to prepend before the scenario starts.</param>
    /// <param name="options">Optional configuration for scenario execution.</param>
    /// <returns>A task that completes when the scenario has finished running.</returns>
    public static Task RunAsync(
        this ChatScenario scenario,
        IChatClient chatClient,
        IChatClient? assertionClient = null,
        IList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(chatClient);

        var runner = new ChatScenarioRunner(assertionClient ?? chatClient);
        return runner.RunAsync(scenario, chatClient, initialMessages, options);
    }

    /// <summary>
    /// Runs the supplied scenarios against the supplied chat client.
    /// </summary>
    /// <param name="scenarios">The scenarios to execute.</param>
    /// <param name="chatClient">The chat client that should process the scenarios.</param>
    /// <param name="assertionClient">Optional chat client used for semantic assertions. Defaults to <paramref name="chatClient"/>.</param>
    /// <param name="initialMessages">Optional chat history to prepend before each scenario starts.</param>
    /// <param name="options">Optional configuration for scenario execution.</param>
    /// <returns>A task that completes when all scenarios have finished running.</returns>
    public static Task RunAsync(
        this IEnumerable<ChatScenario> scenarios,
        IChatClient chatClient,
        IChatClient? assertionClient = null,
        IList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(scenarios);
        ArgumentNullException.ThrowIfNull(chatClient);

        var runner = new ChatScenarioRunner(assertionClient ?? chatClient);
        return runner.RunAsync(scenarios, chatClient, initialMessages, options);
    }

    /// <summary>
    /// Runs the scenario against the supplied agent.
    /// </summary>
    /// <param name="scenario">The scenario to execute.</param>
    /// <param name="agent">The agent that should process the scenario.</param>
    /// <param name="assertionClient">Optional ChatClient used for semantic assertions. Defaults to <paramref name="agent"/>.</param>
    /// <param name="initialMessages">Optional chat history to prepend before the scenario starts.</param>
    /// <param name="options">Optional configuration for scenario execution.</param>
    /// <returns>A task that completes when the scenario has finished running.</returns>
    public static Task RunAsync(
        this ChatScenario scenario,
        AIAgent agent,
        IChatClient? assertionClient = null,
        IList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(agent);

        var runner = new ChatScenarioRunner(assertionClient);
        return runner.RunAsync(scenario, agent, initialMessages, options);
    }

    /// <summary>
    /// Runs the supplied scenarios against the supplied agent.
    /// </summary>
    /// <param name="scenarios">The scenarios to execute.</param>
    /// <param name="agent">The agent that should process the scenarios.</param>
    /// <param name="assertionClient">Optional ChatClient used for semantic assertions. Defaults to <paramref name="agent"/>.</param>
    /// <param name="initialMessages">Optional chat history to prepend before each scenario starts.</param>
    /// <param name="options">Optional configuration for scenario execution.</param>
    /// <returns>A task that completes when all scenarios have finished running.</returns>
    public static Task RunAsync(
        this IEnumerable<ChatScenario> scenarios,
        AIAgent agent,
        IChatClient? assertionClient = null,
        IList<ChatMessage>? initialMessages = null,
        ScenarioRunOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(scenarios);
        ArgumentNullException.ThrowIfNull(agent);

        var runner = new ChatScenarioRunner(assertionClient);
        return runner.RunAsync(scenarios, agent, initialMessages, options);
    }
}
