# skUnit
[![Build and Deploy](https://github.com/mehrandvd/skUnit/actions/workflows/build.yml/badge.svg)](https://github.com/mehrandvd/skUnit/actions/workflows/build.yml)
[![NuGet version (skUnit)](https://img.shields.io/nuget/v/skUnit.svg?style=flat)](https://www.nuget.org/packages/skUnit/)
[![NuGet downloads](https://img.shields.io/nuget/dt/skUnit.svg?style=flat)](https://www.nuget.org/packages/skUnit)

**skUnit** is a testing tool for any `IChatClient` and [SemanticKernel](https://github.com/microsoft/semantic-kernel) units, such as _kernels_, _chat services_ and ...

You can write [**Chat Scenarios**](https://github.com/mehrandvd/skunit/blob/main/docs/chat-scenario-spec.md), which test a sequence of interactions between the user and an `IChatClient` or a SemanticKernel.

# Chat Scenarios

A chat scenario is a way of testing how an `IChatClient`, responds to user inputs in skUnit. 
A chat scenario consists of one or more sub-scenarios, each representing a dialogue turn between the user and the agent.

## Example
This is an example of a chat scenario with two sub-scenarios:

```md
# SCENARIO Height Discussion

## [USER]
Is Eiffel tall?

## [AGENT]
Yes it is

### CHECK SemanticCondition
It agrees that the Eiffel Tower is tall or expresses a positive sentiment.

## [USER]
What about Everest Mountain?

## [AGENT]
Yes it is tall too

### CHECK SemanticCondition
It agrees that Everest mountain is tall or expresses a positive sentiment.
```

![image](https://github.com/mehrandvd/skunit/assets/5070766/156b0831-e4f3-4e4b-b1b0-e2ec868efb5f)

### Sub-scenario 1
The first sub-scenario tests how the agent responds to the question `Is Eiffel tall?`. 
The expected answer is something like `Yes it is`, but this is not an exact match. It is just a guideline for the desired response.

When the scenario is executed, the OpenAI generates an actual answer, such as `Yes it is quite tall.`. 
The next statement `CHECK SemanticCondition` is an assertion that verifies if the actual answer meets the specified condition: 
`It agrees that the Eiffel Tower is tall or expresses a positive sentiment.`

### Sub-scenario 2
The second sub-scenario tests how the agent responds to the follow-up question `What about Everest mountain?`. 
The expected answer is something like `Yes it is tall too`, but again, this is not an exact match. It is just a guideline for the desired response.

When the scenario is executed, the OpenAI generates an actual answer, such as `Yes it is very tall indeed.`. 
The next statement `CHECK SemanticCondition` is an assertion that verifies if the actual answer meets the specified condition: 
`It agrees that Everest mountain is tall or expresses a positive sentiment.`

As you can see, this sub-scenario does not depend on the exact wording of the previous answer. 
It assumes that the agent responded in the expected way and continues the test. 
This makes writing long tests easier, as you can rely on the agent's answers to design your test. 
Otherwise, you would have to account for different variations of the intermediate answers every time you run the test.

However, `SemanticSimilar` is not the only assertion method. There are many more assertion checks available (like **SemanticCondition**, **Equals**). 

You can see the full list of CHECK statements here: [CHECK Statement spec](https://github.com/mehrandvd/skunit/blob/main/docs/check-statements-spec.md).

## Scenarios are Valid Markdowns

One of the benefits of skUnit scenarios is that they are valid **Markdown** files, which makes them very readable and easy to edit. 

> skUnit scenarios are valid **Markdown** files, which makes them very readable and easy to edit.

For example, you can see how clear and simple this scenario is: [Chatting about Eiffel height](https://github.com/mehrandvd/skunit/blob/main/src/skUnit.Tests/SemanticKernelTests/ChatScenarioTests/Samples/EiffelTallChat/skchat.md)

![image](https://github.com/mehrandvd/skunit/assets/5070766/53d009a9-4a0b-44dc-91e0-b0be81b4c5a7)

## Executing a Test Using a Scenario
Executing tests is a straightforward process. You have the flexibility to utilize any preferred test frameworks such as xUnit, nUnit, or MSTest. With just two lines of code, you can load and run a test:

```csharp
var markdown = // Load it from .md file
var scenarios = await ChatScenario.LoadFromText(markdown);
await ScenarioAssert.PassAsync(scenarios,
  getAnswerFunc: async history =>
            {
                var result = // your logic to be tested;
                return result;
            });
```

The test output will be generated incrementally, line by line:

```md
# SCENARIO Height Discussion

## [USER]
Is Eiffel tall?

## [EXPECTED ANSWER]
Yes it is

### [ACTUAL ANSWER]
Yes, the Eiffel Tower in Paris, France, is tall at 330 meters (1,083 feet) in height.

### CHECK Condition
Confirms that the Eiffel Tower is tall or expresses positivity.
✅ OK

## [USER]
What about Everest Mountain?

## [EXPECTED ANSWER]
Yes it is tall too

### [ACTUAL ANSWER]
Yes, Mount Everest is the tallest mountain in the world, with a peak that reaches 29,032 feet (8,849 meters) above sea level.

### CHECK Condition
The sentence is positive.
✅ OK

## [USER]
What about a mouse?

## [EXPECTED ANSWER]
No, it is not tall.

### [ACTUAL ANSWER]
No, a mouse is not tall.

### CHECK Condition
The sentence is negative.
✅ OK

## [USER]
Give me a JSON containing the Eiffel height.
Example: 
{
	"height": "330 meters"
}

## [EXPECTED ANSWER]
{
	"height": "330 meters"
}

### [ACTUAL ANSWER]
{
	"height": "330 meters"
}

### CHECK JsonCheck
{
	"height": ["NotEmpty", ""]
}
✅ OK

### CHECK JsonCheck
{
	"height": ["Contain", "meters"]
}
✅ OK
```

This output is generated line by line as the test is executed:

![image](https://github.com/mehrandvd/skunit/assets/5070766/f3ef8a37-ceab-444f-b6f4-098557b61bfa)


## Documents
To better understand skUnit, Check these documents:
 - [Invocation Scenario Spec](https://github.com/mehrandvd/skunit/blob/main/docs/invocation-scenario-spec.md): The details of writing an InvocationScenario.
 - [Chat Scenario Spec](https://github.com/mehrandvd/skunit/blob/main/docs/chat-scenario-spec.md): The details of writing an ChatScenario.
 - [CHECK Statement Spec](https://github.com/mehrandvd/skunit/blob/main/docs/check-statements-spec.md): The various `CHECK` statements that you can use for assertion.

## Requirements
- .NET 7.0 or higher
- An OpenAI API key

## Installation
You can easily add **skUnit** to your project as it is available as a [NuGet](https://www.nuget.org/packages/skUnit) package. To install it, execute the following command in your terminal:
```bash
dotnet add package skUnit
```

Afterwards, you'll need to instantiate the `SemanticKernelAssert` class in your test constructor. This requires passing your OpenAI subscription details as parameters:
```csharp
public class MyTest
{
  SemanticAssert SemanticAssert { get; set; }
  MyTest(ITestOutputHelper output)
  {
    var chatClient = new AzureOpenAIClient(...);
    SemanticAssert = new SemanticAssert(chatClient);
  }

  [Fact]
  TestChat()
  {
    var scenario = // Load your markdown.
    var scenarios = await ChatScenario.LoadFromTest(scenario);
    await SemanticAssert.PassAsync(scenarios, async history =>
      {
        var result = // your logic to be tested;
        return result;
      });
  }
}
```
And that's all there is to it! 😊
