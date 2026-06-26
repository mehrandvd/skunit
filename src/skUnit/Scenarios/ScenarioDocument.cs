using Microsoft.Extensions.AI;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit.Scenarios;

/// <summary>
/// Runtime-ready scenario model separated from markdown parser structures.
/// </summary>
public sealed class ScenarioDocument
{
    public required string RawText { get; init; }
    public string? Description { get; init; }
    public required IReadOnlyList<ScenarioStep> Steps { get; init; }
}

public sealed class ScenarioStep
{
    public required ChatRole Role { get; init; }
    public required IReadOnlyList<AIContent> Contents { get; init; }
    public required IReadOnlyList<IChatAssertion> Assertions { get; init; }
    public string Text => string.Join("\n", Contents.OfType<TextContent>().Select(c => c.Text));

    public ChatMessage ToChatMessage()
    {
        return new ChatMessage(Role, Contents.ToList());
    }
}
