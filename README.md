# skUnit
[![Build and Deploy](https://github.com/mehrandvd/skUnit/actions/workflows/build.yml/badge.svg)](https://github.com/mehrandvd/skUnit/actions/workflows/build.yml)
[![NuGet version (skUnit)](https://img.shields.io/nuget/v/skUnit.svg?style=flat)](https://www.nuget.org/packages/skUnit/)
[![NuGet downloads](https://img.shields.io/nuget/dt/skUnit.svg?style=flat)](https://www.nuget.org/packages/skUnit)

**skUnit** is a testing tool for [SemanticKernel](https://github.com/microsoft/semantic-kernel) units, such as plugin functions and kernels.

For example, you can use skUnit to test a `GetSentiment` function that analyzes a text and returns its sentiment, such as _"Happy"_ or _"Sad"_.
You can write different scenarios to check how the function behaves with various inputs, such as:

```md
## PARAMETER input
Such a beautiful day it is

## ANSWER Equals
Happy
```

This scenario verifies that the function returns _"Happy"_ when the input is _"Such a beautiful day it is"_.

This is an **Invocation Scenario**, which tests a single function call. You can also write **Chat Scenarios**, which test a sequence of interactions between the user and the SemanticKernel.

One of the benefits of skUnit scenarios is that they are valid **Markdown** files, which makes them very readable and easy to edit. For example, you can see how clear and simple this scenario is: [Chatting about Eiffel height](https://github.com/mehrandvd/skunit/blob/main/src/skUnit.Tests/SemanticKernelTests/ChatScenarioTests/Samples/EiffelTallChat/skchat.md)

> skUnit scenarios are valid **Markdown** files, which makes them very readable and easy to edit.

skUnit offers many features to help you write more complex and flexible scenarios. In this section, we will show you some of them with an example.

Suppose you have a function called `GetSentiment` that takes two parameters and returns a sentence describing the sentiment of the text:

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

The most interesting part of this scenario is:

```md
## ANSWER SemanticSimilar
The sentiment is happy
```
This line specifies the expected output of the function and how to compare it with the actual output. 
In this case, the output should be **semantically similar** to _"The sentiment is happy"_.
This means that the output can have different words or syntax, but the meaning should be the same.

> This is a powerful feature of skUnit scenarios, as **it allows you to use OpenAI itself to perform semantic comparisons**.

You can also write this assertion in another way:

```md
## ANSWER
The sentiment of the sentence is happy

## CHECK SemanticSimilar
The sentiment is happy
```

In this style, the expected answer is just a reminder and not used for comparison; 
and then a `## CHECK SemanticSimilar` is used to explicitly perform the assertion.

However, `SemanticSimilar` is not the only assertion method. There are many more assertion checks available (like **SemanticCondition**, **Equals**). 

You can see the full list of CHECK statements here: [CHECK Statement spec](https://github.com/mehrandvd/skunit/blob/main/docs/check-statements-spec.md).

## Executing a Test Using a Scenario
Executing tests is a straightforward process. You have the flexibility to utilize any preferred test frameworks such as xUnit, nUnit, or MSTest. With just two lines of code, you can load and run a test:

```csharp
var scenarios = InvocationScenario.LoadFromText(scenario);
await SemanticKernelAssert.CheckScenarioAsync(Kernel, scenarios);
```

The standout feature of skUnit is its detailed test output. Here's an example:

```md
# SCENARIO GetSentimentHappy_Fail

## PARAMETER input
You are such a bastard, Fuck off!

## PARAMETER options
happy, angry

## EXPECTED ANSWER
The sentiment is happy

## ACTUAL ANSWER
angry

## ANSWER SemanticSimilar
The sentiment is happy
Exception as EXPECTED:
The two texts are not semantically equivalent. The first text expresses anger, while the second text expresses happiness.
```

> As demonstrated, when a `SemanticSimilar` check fails, it provides a semantic explanation for the failure. This feature proves to be incredibly useful during debugging.

Here's another example of an executed test:
![image](https://github.com/mehrandvd/skunit/assets/5070766/37cf7232-e187-41dc-827d-d9630ce86c71)


## Requirements
- .NET 8.0 or higher
- An OpenAI API key


Here's a refined version of the text:

## Installation
You can easily add **skUnit** to your project as it is available as a [NuGet](https://www.nuget.org/packages/skUnit) package. To install it, execute the following command in your terminal:
```bash
dotnet add package skUnit
```

Afterwards, you'll need to instantiate the `SemanticKernelAssert` class in your test constructor. This requires passing your OpenAI subscription details as parameters:
```csharp
public class MyTest
{
  SemanticKernelAssert SemanticKernelAssert { get; set; }
  MyTest(ITestOutputHelper output)
  {
    SemanticKernelAssert = new SemanticKernelAssert(_deploymentName, _endpoint, _apiKey, message => output.WriteLine(message));
  }

  [Fact]
  MyFunctionShouldWork()
  {
    var scenarios = await InvocationScenario.LoadFromResourceAsync(scenario);
    await SemanticKernelAssert.CheckScenarioAsync(Kernel, scenarios);
  }
}
```
And that's all there is to it! 😊
