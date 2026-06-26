# ScenarioRunOptions: Mitigating Model Hallucinations

Large language models are **stochastic** – the same prompt may produce slightly different outputs on different runs. A single failing run (a "hallucination") can cause otherwise good tests to intermittently fail. `ScenarioRunOptions` lets you make your semantic tests **resilient** by:

1. Executing a scenario multiple times
2. Requiring that a configurable number of runs succeed

## Properties

| Property | Default | Meaning |
|----------|---------|---------|
| `TotalRuns` | `1` | How many complete executions of the scenario to perform |
| `RequiredSuccessRuns` | `null` | How many successful runs are required for the scenario to pass; `null` means all runs must pass |

> A run "passes" when all CHECK assertions succeed for that execution.

## Basic Example

```csharp
var options = new ScenarioRunOptions
{
    TotalRuns = 3,
    RequiredSuccessRuns = 2 // At least 2 of 3 must pass
};

await ScenarioAssert.PassAsync(scenarios, chatClient, options: options);
```

## Why This Helps

| Problem | Without Options | With ScenarioRunOptions |
|---------|-----------------|--------------------------|
| Occasional hallucination | Whole test suite flakes | Majority vote smooths noise |
| Non-deterministic temperature settings | Hard to tune for stability | Keep creativity while enforcing reliability |
| Evaluating tool / function calling | One-off tool omission fails build | Only fails if pattern repeats |

## Recommended Settings

| Scenario Type | Suggested Settings | Rationale |
|---------------|--------------------|-----------|
| Deterministic (low temperature) | `TotalRuns = 1`, `RequiredSuccessRuns = 1` | Fast feedback |
| Creative copy / marketing text | `TotalRuns = 5`, `RequiredSuccessRuns = 3` | Allow variation, catch systemic issues |
| Tool / Function invocation | `TotalRuns = 3`, `RequiredSuccessRuns = 2` | Ensure reliability of structured calls |
| Regression CI (critical path) | `TotalRuns = 5`, `RequiredSuccessRuns = 4` | High confidence before shipping |

## Pattern: Progressive Hardening

Start strict (require 100%) while iterating. If you observe rare, benign variations, *lower* only `RequiredSuccessRuns` slightly instead of removing assertions.

```csharp
// Phase 1 (authoring): require perfection
new ScenarioRunOptions { TotalRuns = 1, RequiredSuccessRuns = 1 };

// Phase 2 (stabilizing): allow one-off drift
new ScenarioRunOptions { TotalRuns = 3, RequiredSuccessRuns = 2 };

// Phase 3 (mature): statistically strong signal
new ScenarioRunOptions { TotalRuns = 5, RequiredSuccessRuns = 4 };
```

## Interpreting Failures

Failure message example:
```
Only 2 of 5 runs passed, which is below the required success runs of 4.
```
Indicates the model produced unacceptable answers in a *repeatable pattern* (not just noise) – investigate prompt, model, or assertions.

## Best Practices

- Keep `TotalRuns` small locally; increase in CI if needed
- Combine with **FunctionCall** and **JsonCheck** to enforce structure while allowing linguistic freedom
- Track historical pass rate drift to detect model regression
- Use higher thresholds for safety-sensitive outputs

---
**TL;DR:** `ScenarioRunOptions` adds statistical robustness to semantic tests, reducing flakes while still catching real regressions.
