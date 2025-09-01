# SCENARIO Time Discussion

## [USER]
What time is it?

## [ASSISTANT]
10:23

### ASSERT SemanticCondition
It mentions a time.

### ASSERT FunctionCall
```json
{
	"function_name": "TimePlugin_GetCurrentTime",
}
```
