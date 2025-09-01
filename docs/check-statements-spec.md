# CHECK Statement and ASSERT Keyword

The `CHECK` statement (and the new `ASSERT` keyword) is used to **verify** the quality of an output by comparing it with some **criteria**. Both keywords work identically - use whichever you prefer:

```md
## ANSWER
Yes it is a very good day

### CHECK SemanticCondition
Its vibe is positive

### ASSERT SemanticCondition
Its vibe is positive

### CHECK SemanticSimilar
It is a beautiful day

### ASSERT SemanticSimilar
It is a beautiful day

### CHECK Contains
day

### ASSERT ContainsAll
day

### CHECK Equals
Yes it is a very good day

### ASSERT Equals
Yes it is a very good day
```

Both `CHECK` and `ASSERT` keywords work identically. The `ASSERT` keyword was introduced for better readability and consistency with common testing terminology.

In this example, the expected answer is something like "Yes it is a very good day", but the actual answer could be anything. To verify the actual output, you can use different types of checks. In this document, we are going to **explain** each of them in detail.

## CHECK Equals
This is the most basic check. It checks if the answer is **exactly the same** as the expected one.
```md
### CHECK Equals
Happy
```
This statement checks if the output exactly equals `Happy`.

For this statement, `Equals` and `Equal` are both accepted.

## CHECK ContainsAll
It checks if the output contains **all** of the items in the text (comma-separated).
```md
### CHECK ContainsAll
happy, day, good
```
This check passes if the output contains all the words: happy, day, and good.

For this statement, all these are the same: `Contains`, `Contain`, `ContainsAll`

## CHECK ContainsAny
It checks if the output contains **any** of the items in the text (comma-separated).
```md
### CHECK ContainsAny
happy, day, good
```
This check passes if the output contains any of the words: happy, day, or good.

## CHECK SemanticSimilar
It checks if the output is **similar in meaning** to the given text.
```md
### CHECK SemanticSimilar
It is a good day.
```
This check passes if the output is semantically similar to "It is a good day". It uses OpenAI tokens to check this assertion.

If it fails while executing the test, it **shows** why these two texts are not semantically equivalent with a message like this:
```md
## ANSWER SemanticSimilar
The sentiment is happy
Exception as EXPECTED:
The two texts are not semantically equivalent. The first text expresses anger, while the second text expresses happiness.
```

For this statement, all these are the same: `SemanticSimilar`, `Semantic-Similar`

## CHECK SemanticCondition
It checks if the output **satisfies** the condition semantically.
```md
### CHECK SemanticCondition
It talks about a good day.
```
This check passes if the output semantically meets the condition: "It talks about a good day". It uses OpenAI tokens to check this assertion.

If it fails while executing the test, it **shows** why the output does not meet the condition semantically with a message like this:
```md
### CHECK SemanticCondition
It talks about trees
Exception as EXPECTED:
The input text does not talk about trees
```

For this statement, all these are the same: `SemanticCondition`, `Semantic-Condition`

## CHECK JsonCheck
It checks if the output is a valid JSON, and if each of its properties meets the specified condition.
```md
### CHECK JsonCheck
{
  "name": ["Equals", "Mehran"],
  "description": ["SemanticCondition", "It mentions that he is a good software architect."]
}
```
This check passes if the output is a valid JSON and:
 - It has a `name` property with a value equal to `Mehran`
 - It has a `description` property with a value that meets this semantic condition: "It mentions that he is a good software architect."

This check is great for asserting functions that are going to return a specific JSON.

To make your `.md` files more readable, you can annotate your JSON with ` ``` `:

``````md
### CHECK JsonCheck
```json
{
  "name": ["Equals", "Mehran"],
  "description": ["SemanticCondition", "It mentions that he is a good software architect."]
}
```
``````

## CHECK Empty
It ensures that the answer is empty.
```md
### CHECK Empty
```
This statement checks if the output is empty.

## CHECK NotEmpty
It ensures that the answer is not empty.
```md
### CHECK NotEmpty
```
This statement checks if the output is empty.

## CHECK FunctionCall
It ensures that a function call happens during answer generation.

``````md
### CHECK FunctionCall
```json
{
  "function_name": "GetFoodMenu"
}
```
``````

This statement checks if `GetFoodMenu` function has been called during the answer generation.
Also, the following syntax can be used as a sugar syntactic.

```md
### CHECK FunctionCall
GetFoodMenu
```

Also you can use some more advanced assertions by checking the called arguments using arguments conditions. Argument condition is an array that contains the name of the condition as the first item, followed by the values:

``````md
### CHECK FunctionCall
```json
{
  "function_name": "GetFoodMenu",
  "arguments": {
    "mood": ["Equals", "Happy"]
  }
}
```
``````

There are several condtions supported.
### Equals
It checks if the argument is equal to the specified value:
``````md
### CHECK FunctionCall
```json
{
  "function_name": "GetFoodMenu",
  "arguments": {
    "mood": ["Equals", "Happy"]
  }
}
```
``````

### Empty
It checks if the argument is null or empty:
``````md
### CHECK FunctionCall
```json
{
  "function_name": "GetFoodMenu",
  "arguments": {
    "mood": ["Empty"]
  }
}
```
``````

### NotEmpty
It checks if the argument is not null or empty:
``````md
### CHECK FunctionCall
```json
{
  "function_name": "GetFoodMenu",
  "arguments": {
    "mood": ["NotEmpty"]
  }
}
```
``````

### IsAnyOf
It checks whether the argument value is equal to any of the specified values:
``````md
### CHECK FunctionCall
```json
{
  "function_name": "GetFoodMenu",
  "arguments": {
    "mood": ["IsAnyOf", "Happy", "Sad", "Angry"]
  }
}
```
``````

### ContainsAny
It checks if the argument value contains any of the specified items:
``````md
### CHECK FunctionCall
```json
{
  "function_name": "GetFoodMenu",
  "arguments": {
    "mood": ["ContainsAny", "Happy", "Sad", "Angry"]
  }
}
```
``````

### ContainsAll
It checks if the argument value contains all of the specified items:
``````md
### CHECK FunctionCall
```json
{
  "function_name": "GetFoodMenu",
  "arguments": {
    "mood": ["ContainsAll", "Happy", "Sad", "Angry"]
  }
}
```
``````

### SemanticCondition
It checks if the argument value satisfies the condition semantically:
``````md
### CHECK FunctionCall
```json
{
  "function_name": "GetFoodMenu",
  "arguments": {
    "mood": ["SemanticCondition", "Is happy"]
  }
}
```
``````

### SemanticSimilar
It checks if the argument value is similar in meaning to the given text:
``````md
### CHECK FunctionCall
```json
{
  "function_name": "GetFoodMenu",
  "arguments": {
    "mood": ["SemanticSimilar", "Is happy"]
  }
}
```


``````