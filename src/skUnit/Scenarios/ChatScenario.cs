using Markdig.Helpers;
using Microsoft.SemanticKernel.ChatCompletion;
using skUnit.Scenarios.Parsers;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit.Scenarios;

/// <summary>
/// A ChatScenario is a scenario to test a ChatCompletionService, Kernel, Function or Plugin.
/// It contains the required inputs to call a InvokeAsync like: ChatItems.
/// Also it contains the expected output for each level of chat by: Assertions
/// </summary>
public class ChatScenario : Scenario<ChatScenario, ChatScenarioParser>
{
    public string? Description { get; set; }
    public required string RawText { get; set; }

    /// <summary>
    /// The ChatItems that should be applied to chat history one by one and
    /// check for assertions for each level.
    /// </summary>
    public List<ChatItem> ChatItems { get; set; } = new();

}

public class ChatItem
{
    public ChatItem(AuthorRole role, string content)
    {
        Role = role;
        Content = content;
    }
    public AuthorRole Role { get; set; }
    public string Content { get; set; }

    /// <summary>
    /// All the assertions that should be checked after the result of InvokeAsync is ready for the user input and history so far.
    /// </summary>
    public List<IKernelAssertion> Assertions { get; set; } = new();

    /// <summary>
    /// The function calls that should be asserted too.
    /// </summary>
    public List<FunctionCall> FunctionCalls { get; set; } = new();

    public override string ToString()
    {
        return $"{Role}: {Content}";
    }
}

public class FunctionCall
{
    public required string PluginName { get; set; }
    public required string FunctionName { get; set; }
    public List<FunctionCallArgument> Arguments { get; set; } = new();
    public string? ArgumentsText { get; set; }

    /// <summary>
    /// All the assertions that should be checked after the result of InvokeAsync is ready.
    /// </summary>
    public List<IKernelAssertion> Assertions { get; set; } = new();

    public override string ToString() => $"{PluginName}{FunctionName}({string.Join(",", Arguments.Select(a => a.Name))})";
}

public class FunctionCallArgument
{
    public required string Name { get; set; }
    public string? LiteralValue { get; set; }
    public string? InputVariable { get; set; }

    public override string ToString() =>
        InputVariable != null ? $"{Name}=${InputVariable}"
            : $"{Name}=\"{LiteralValue}\"";
}