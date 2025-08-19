using Markdig.Helpers;
using Microsoft.Extensions.AI;
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
    public ChatItem(ChatRole role, string content)
    {
        Role = role;
        // Maintain backward compatibility: convert string content to TextContent
        Contents = new List<AIContent> { new TextContent(content) };
    }

    public ChatItem(ChatRole role, List<AIContent> contents)
    {
        Role = role;
        Contents = contents;
    }

    public ChatRole Role { get; set; }
    
    /// <summary>
    /// The AI content parts for this chat item (text, images, etc.)
    /// </summary>
    public List<AIContent> Contents { get; set; } = new();

    /// <summary>
    /// Backward compatibility property that returns combined text content
    /// </summary>
    public string Content 
    { 
        get => string.Join("\n", Contents.OfType<TextContent>().Select(t => t.Text));
        set => Contents = new List<AIContent> { new TextContent(value) };
    }

    /// <summary>
    /// All the assertions that should be checked after the result of InvokeAsync is ready for the user input and history so far.
    /// </summary>
    public List<IKernelAssertion> Assertions { get; set; } = new();

    /// <summary>
    /// The function calls that should be asserted too.
    /// </summary>
    public List<FunctionCall> FunctionCalls { get; set; } = new();

    /// <summary>
    /// Convert to Microsoft.Extensions.AI ChatMessage
    /// </summary>
    /// <returns>ChatMessage with all content parts</returns>
    public ChatMessage ToChatMessage()
    {
        return new ChatMessage(Role, Contents);
    }

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