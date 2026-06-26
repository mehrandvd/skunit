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

    public ScenarioDocument ToDocument()
    {
        return new ScenarioDocument
        {
            RawText = RawText,
            Description = Description,
            Steps = ChatItems.Select(chatItem => new ScenarioStep
            {
                Role = chatItem.Role,
                Contents = chatItem.Contents.ToList(),
                Assertions = chatItem.Assertions.ToList(),
            }).ToList(),
        };
    }
}

public class ChatItem
{
    public ChatItem(ChatRole role, string text)
    {
        Role = role;
        Contents = new List<AIContent> { new TextContent(text) };
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
    /// Combined text content from all text parts in this item.
    /// </summary>
    public string Text
    {
        get => string.Join("\n", Contents.OfType<TextContent>().Select(t => t.Text));
        set => Contents = new List<AIContent> { new TextContent(value) };
    }

    /// <summary>
    /// All the assertions that should be checked after the result of InvokeAsync is ready for the user input and history so far.
    /// </summary>
    public List<IChatAssertion> Assertions { get; set; } = new();


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
        return $"{Role}: {Text}";
    }
}
