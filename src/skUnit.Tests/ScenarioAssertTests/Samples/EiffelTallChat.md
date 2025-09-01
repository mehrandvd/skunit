# SCENARIO Height Discussion

## [USER]
Is Eiffel tall?

## CALL MyPlugin.GetIntent
```json
{
	"options": "Happy,Question,Sarcastic"
}
```

## ASSERT ContainsAny
Question

## [ASSISTANT]
Yes it is

### ASSERT Condition
Approves that eiffel tower is tall or is positive about it.

## CALL MyPlugin.GetIntent
```json
{
	"options": "Positive,Negative,Neutral"
}
```
## ASSERT ContainsAny
Neutral,Positive

## [USER]
What about everest mountain?

## [ASSISTANT]
Yes it is tall too

### ASSERT Condition
The sentence is positive.

## [USER]
What about a mouse?

## [ASSISTANT]
No it is not tall.

### ASSERT Condition
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

### ASSERT JsonStructure
{
	"height": ["NotEmpty", ""]
}

### ASSERT JsonStructure
{
	"height": ["Contain", "meters"]
}

