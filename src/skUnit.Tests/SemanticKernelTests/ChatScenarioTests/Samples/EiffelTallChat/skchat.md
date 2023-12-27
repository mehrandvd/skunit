# SCENARIO Height Discussion

## [USER]
Is Eiffel tall?

## [AGENT]
Yes it is

### CHECK SemanticCondition
Approves that eiffel tower is tall or is positive about it.

## [USER]
What about everest mountain?

## [AGENT]
Yes it is tall too

### CHECK SemanticCondition
The sentence is positive.

## [USER]
What about a mouse?

## [AGENT]
No it is not tall.

### CHECK SemanticCondition
The sentence is negative.

## [USER]
Give me a json containing the Eiffel height.

Example: 
{
	"height": "330 meters"
}

## [AGENT]
{
	"height": "330 meters"
}

### CHECK JsonCheck
{
	"height": ["Contain", "meters"]
}

