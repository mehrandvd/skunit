# Multi-Modal Content Support

skUnit now supports multi-modal content in chat scenarios, allowing you to include images alongside text in your test scenarios. This enables testing of AI models that support vision capabilities, such as GPT-4o.

## Syntax

Within any chat role block (`[USER]`, `[AGENT]`, `[SYSTEM]`, `[TOOL]`), you can define multiple content parts using subsections:

### Text Content
```markdown
### Text
Your text content here...
```

### Image Content
```markdown
### Image
![Alt text](https://example.com/image.jpg)
```

## Example

Here's a complete example of a multi-modal scenario:

```markdown
# SCENARIO Multi-modal Image Analysis

## [USER]
### Text
This image explains how skUnit parses the chat scenarios.
### Image
![skUnit structure](https://github.com/mehrandvd/skunit/assets/5070766/156b0831-e4f3-4e4b-b1b0-e2ec868efb5f)
### Text
How many scenarios are there in the picture?

## [AGENT]
There are 2 scenarios in the picture

### CHECK SemanticSimilar
There are 2 scenarios in the picture
```

## Backward Compatibility

The new multi-modal support is fully backward compatible. Existing scenarios that don't use subsections will continue to work exactly as before:

```markdown
# SCENARIO Traditional Text-Only

## [USER]
Just plain text without subsections

## [AGENT]
Plain text response
```

## Technical Details

- **Text Content**: Handled by `TextContentPart` class
- **Image Content**: Handled by `ImageContentPart` class, supporting any web-accessible image URL
- **AI Integration**: Content parts are automatically converted to `Microsoft.Extensions.AI` content types (`TextContent`, `UriContent`)
- **Extensibility**: The `ChatContentPart` base class allows for future content types (audio, video, etc.)

## Usage in Tests

When using multi-modal scenarios in your tests, the content is automatically converted to the appropriate format for the underlying AI service:

```csharp
var scenarios = ChatScenario.LoadFromText(scenarioText);
var chatItem = scenarios.First().ChatItems.First();

// Access individual content parts
var textParts = chatItem.ContentParts.OfType<TextContentPart>();
var imageParts = chatItem.ContentParts.OfType<ImageContentPart>();

// Convert to AI framework format
var chatMessage = chatItem.ToChatMessage(); // Returns Microsoft.Extensions.AI.ChatMessage
```

## Future Extensions

The infrastructure is designed to support additional content types in the future:
- Audio content (`### Audio`)
- Video content (`### Video`) 
- File attachments (`### File`)
- And more...

Each new content type can be added by creating a new `ChatContentPart` subclass and updating the parser accordingly.