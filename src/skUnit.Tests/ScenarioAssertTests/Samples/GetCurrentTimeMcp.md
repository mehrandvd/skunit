# SCENARIO Time Discussion

## [USER]
What time is it?

## [AGENT]
10:23

### CHECK SemanticCondition
It mentions a time.

### CHECK FunctionCall
```json
{
	"function_name": "current_time",
}
```

## [USER]
How many days are in this year's january?

## [AGENT]
31 days

### CHECK SemanticCondition
It mentions 31 days.

### CHECK FunctionCall
```json
{
	"function_name": "days_in_month",
}
```
