# SCENARIO AngryBastard

## PROMPT
What is sentiment of this input text, your options are: {{$options}}
                
[[input text]]

{{$input}}

[[end of input text]]
                
just result the sentiment without any spaces.

SENTIMENT: 

## PARAMETER input
You are such a bastard, Fuck off!

## PARAMETER options
happy,angry

## ANSWER
The sentiment is angry

### CHECK Contains
angry

### CHECK Equals
angry

---------------------------------

# SCENARIO NeverComeBack

## PROMPT
What is sentiment of this input text, your options are: {{$options}}
                
[[input text]]

{{$input}}

[[end of input text]]
                
just result the sentiment without any spaces.

SENTIMENT:
## PARAMETER input
Go away and never come back to me, you laier!

## PARAMETER options
happy,angry

## ANSWER
The sentiment is angry

### CHECK