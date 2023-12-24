using Microsoft.SemanticKernel.ChatCompletion;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit.Scenarios;

public class ChatScenario : Scenario
{
    public string? Description { get; set; }
    public required string RawText { get; set; }
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
    public List<IKernelAssertion> Assertions { get; set; } = new();

    public override string ToString()
    {
        return $"{Role}: {Content}";
    }
}