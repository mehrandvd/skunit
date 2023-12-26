# Invocation Scenario Specification
## What is an invocation scenario?
An invocation scenario is a way of testing SemanticKernel units, such as *plugin functions* and *kernels*, in skUnit. 
A scenario consists of providing some input to a unit, invoking it, and checking the expected output.

For example, suppose you have a `GetSentiment` function that takes a text and returns its sentiment, such as _"Happy"_ or _"Sad"_.
You can test this function with different scenarios, such as:

```md
## PARAMETER input
Such a beautiful day it is

## ANSWER Equals
Happy
```

This scenario verifies that the function returns _"Happy"_ when the input is _"Such a beautiful day it is"_.

If the function only has one parameter, you can omit the `## PARAMETER input` line and write the input directly, like this:

```md
Such a beautiful day it is

## ANSWER Equals
Happy
```

This scenario is equivalent to the previous one.

## Scenario Features
You can use various features to define more complex scenarios. In this section, we will explain some of them with an example.

Let's say you have a function called `GetSentiment` that takes two parameters and returns a sentence describing the sentiment of the text:

**Parameters**:
  - _input_: the text to analyze
  - _options_: the possible sentiment values, such as _happy_, _angry_, or _sad_
  
**Returns**: a sentence like _"The sentiment is happy"_ or _"The sentiment of this text is sad"_.

Here is a scenario that tests this function:

```md
# SCENARIO GetSentimentHappy

## PARAMETER input
Such a beautiful day it is

## PARAMETER options
happy, angry

## ANSWER SemanticSimilar
The sentiment is happy
```

Let's break down this scenario line by line.

```md
# SCENARIO GetSentimentHappy
```
This line gives a name or description to the scenario, which can be used later to generate xUnit theories.

```md
## PARAMETER input
Such a beautiful day it is

## PARAMETER options
happy, angry
```
These lines define the values of the parameters that will be passed to the function, such as:

```csharp
var actual = await function.InvokeAsync<string>(kernel, 
  arguments: new()
  {
    ["input"] = "Such a beautiful day it is",
    ["options"] = "happy,angry"
  });
```

```md
## ANSWER SemanticSimilar
The sentiment is happy
```
This line specifies the expected output of the function and how to compare it with the actual output. 
In this case, the output should be **semantically similar** to _"The sentiment is happy"_.
This means that the output can have different words or syntax, but the meaning should be the same.

> This is a powerful feature of skUnit scenarios, as **it allows you to use OpenAI itself to perform semantic comparisons**.

This assertion can also be written in an alternative style:

```md
## ANSWER
The sentiment of the sentence is happy

## CHECK SemanticSimilar
The sentiment is happy
```

In this style, an expected answer is just written to serve as a reminder of the anticipated answer (which will not be used during assertions); 
and then a `## CHECK SemanticSimilar` is used to explicitly perform the assertion.

However, `SemanticSimilar` is not the only assertion method. There are many more assertions available (like SemanticCondition, Equals, ...), which will be discussed later.
