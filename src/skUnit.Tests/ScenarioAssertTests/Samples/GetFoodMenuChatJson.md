# SCENARIO Time Discussion

## [USER]
What a beautiful day. What food do your menu?

## [ASSISTANT]
Pizza

### ASSERT ToolCall
```json
{
	"function_name": "GetFoodMenu",
	"arguments": {
		"mood": ["Equals", "Happy"]
	}
}
```
