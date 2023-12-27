# CHECK Statement
The `CHECK` statement is used to **verify** the quality of an output by comparing it with some **criteria**. For example:
```md
## ANSWER
Yes it is a very good day

### CHECK SemanticCondition
Its vibe is positive

### CHECK SemanticSimilar
It is a beautiful day

### CHECK Contains
day

### CHECK Equals
Yes it is a very good day
```

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
## CHECK SemanticCondition
It talks about trees
Exception as EXPECTED:
The input text does not talk about trees
```

For this statement, all these are the same: `SemanticCondition`, `Semantic-Condition`
