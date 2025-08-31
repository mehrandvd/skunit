# SCENARIO Height Discussion

## [USER]
Is Eiffel tall?

## CALL MyPlugin.GetIntent
```json
{
	"options": "Happy,Question,Sarcastic"
}
```

## CHECK ContainsAny
Question

## [ASSISTANT]
Yes it is

### CHECK SemanticCondition
Approves that eiffel tower is tall or is positive about it.

## CALL MyPlugin.GetIntent
```json
{
	"options": "Positive,Negative,Neutral"
}
```
## CHECK ContainsAny
Neutral,Positive

## [USER]
What about everest mountain?

## [ASSISTANT]
Yes it is tall too

### CHECK SemanticCondition
The sentence is positive.

## [USER]
What about a mouse?

## [ASSISTANT]
No it is not tall.

### CHECK SemanticCondition
The sentence is negative or mentions that mouse is not tall.

## [USER]
Give me a json containing the Eiffel height.

Example: 
{
	"height": "330 meters"
}

## [ASSISTANT]
{
	"height": "330 meters"
}

### CHECK JsonCheck
{
	"height": ["NotEmpty", ""]
}

### CHECK JsonCheck
{
	"height": ["Contain", "meters"]
}

