using Microsoft.Extensions.AI;

namespace skUnit.Scenarios.ContentParts;

/// <summary>
/// Base class for chat content parts that can be included in a ChatItem.
/// This enables multi-modal content support (text, images, etc.)
/// </summary>
public abstract class ChatContentPart
{
    /// <summary>
    /// Convert this content part to a Microsoft.Extensions.AI AIContent for chat completion
    /// </summary>
    /// <returns>AIContent instance</returns>
    public abstract AIContent ToAIContent();
}

/// <summary>
/// Represents text content in a chat message
/// </summary>
public class TextContentPart : ChatContentPart
{
    public required string Text { get; set; }

    public TextContentPart() { }

    public TextContentPart(string text)
    {
        Text = text;
    }

    public override AIContent ToAIContent()
    {
        return new TextContent(Text);
    }

    public override string ToString() => Text;
}

/// <summary>
/// Represents image content in a chat message using a URI/URL
/// </summary>
public class ImageContentPart : ChatContentPart
{
    public required string ImageUri { get; set; }
    public string? AltText { get; set; }

    public ImageContentPart() { }

    public ImageContentPart(string imageUri, string? altText = null)
    {
        ImageUri = imageUri;
        AltText = altText;
    }

    public override AIContent ToAIContent()
    {
        return new UriContent(new Uri(ImageUri), "image/*");
    }

    public override string ToString() => $"Image: {ImageUri}";
}