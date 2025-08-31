# Chat Scenario Spec
A chat scenario is a way of testing how SemanticKernel units, such as plugin functions and kernels, respond to user inputs in skUnit. 
A chat scenario consists of one or more sub-scenarios, each representing a dialogue turn between the user and the agent.

There are two types of scenarios: [Invocation Scenario](https://github.com/mehrandvd/skunit/blob/main/docs/invocation-scenario-spec.md) and Chat Scenario.
An invocation scenario is a simpler form of testing a single input-output pair. If you are not familiar with it, please read its documentation first.

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

For a more complex chat scenario, you can refer to this file that is used as a scenario to pass skUnit unit tests:
[Eiffel Tall Chat](https://github.com/mehrandvd/skunit/blob/main/src/skUnit.Tests/SemanticKernelTests/ChatScenarioTests/Samples/EiffelTallChat/skchat.md)

```md
# SCENARIO Height Discussion

## [USER]
Is Eiffel tall?

## [AGENT]
Yes it is

### CHECK SemanticCondition
Confirms that the Eiffel tower is tall or expresses positivity.

## [USER]
What about Everest Mountain?

## [AGENT]
Yes it is tall too

### CHECK SemanticCondition
The sentence is positive.

## [USER]
What about a mouse?

## [AGENT]
No, it is not tall.

### CHECK SemanticCondition
The sentence is negative.

## [USER]
Give me a JSON containing the Eiffel height.

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
	"height": ["NotEmpty", ""]
}

### CHECK JsonCheck
{
	"height": ["Contain", "meters"]
}
```

## Test Execution
Here's what you can expect to see in the output when running this test using skUnit as a xUnit `[Fact]`:

```csharp
var scenarios = await LoadChatScenarioAsync("EiffelTallChat");
await SemanticKernelAssert.CheckChatScenarioAsync(Kernel, scenario);
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


## Advanced Scenario Features

### Flexible Use of Hashtags
When defining skUnit statements such as `# SCENARIO`, `## [USER]`, and so on, you have the freedom to use as many hashtags as you wish. There's no strict rule that mandates a specific count of hashtags for each statement. This flexibility allows you to format your markdown in a way that enhances readability for you. However, as a best practice, we suggest adhering to the recommended usage to maintain a clear and comprehensible hierarchy.

### Assistant Alias Support
For better alignment with Microsoft Extensions AI (MEAI) standards, skUnit supports `[ASSISTANT]` as an alias for `[AGENT]`. Both forms are functionally equivalent and map to the same assistant role:

```md
# SCENARIO Using AGENT

## [USER]
What is the capital of France?

## [AGENT]
The capital of France is Paris.
```

```md
# SCENARIO Using ASSISTANT

## [USER]
What is the capital of France?

## [ASSISTANT]
The capital of France is Paris.
```

You can even mix both forms within the same scenario:

```md
# SCENARIO Mixed Usage

## [USER]
Hello

## [AGENT]
Hi there!

## [USER]
How are you?

## [ASSISTANT]
I'm doing well, thank you!
```

### Unique Identifiers
In certain uncommon instances, the data may contain skUnit expressions that could disrupt the parsing of the scenario. For instance, let's consider a scenario with two chat items. If the first chat item contains a markdown value that disrupts parsing, it could pose a problem:

```md
# SCENARIO

## [USER]
This block itself contains a chat:
	## [USER]
	Hello
	## [AGENT]
	Hi, How can I help you?

## [AGENT]
Wow, this is a chat.
```

To handle these exceptional cases, you can employ an identifier in your statements, like so:

```md
# sk SCENARIO

## sk [USER]
This block itself contains a chat:
	## [USER]
	Hello
	## [AGENT]
	Hi, How can I help you?

## sk [AGENT]
Wow, this is a chat.
```

In this example, we used `sk` as the identifier. However, you can use any identifier of your choice, such as `~`, `*`, etc. The parser will recognize whatever you use in the first statement as the unique identifier for the statements.

