# SCENARIO Time Discussion

## [USER]
What a beautiful day. What food do your menu?

## [AGENT]
Pizza

### CHECK FunctionCall
```json
{
	"function_name": "GetFoodMenu",
	"arguments": {
		"mood": ["Equals", "Happy"]
	}
}
```
