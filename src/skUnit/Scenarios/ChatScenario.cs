using Microsoft.SemanticKernel.ChatCompletion;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit.Scenarios;

/// <summary>
/// An InvokeScenario is a scenario to test a ChatCompletionService, Kernel, Function or Plugin.
/// It contains the required inputs to call a InvokeAsync like: ChatItems.
/// Also it contains the expected output for each level of chat by: Assertions
/// </summary>
public class ChatScenario
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

    public override string ToString()
    {
        return $"{Role}: {Content}";
    }
}