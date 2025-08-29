# ScenarioRunOptions: Mitigating Model Hallucinations

Large language models are **stochastic** – the same prompt may produce slightly different outputs on different runs. A single failing run (a "hallucination") can cause otherwise good tests to intermittently fail. `ScenarioRunOptions` lets you make your semantic tests **resilient** by:

1. Executing a scenario multiple times
2. Requiring that only a configurable percentage of runs succeed

## Properties

| Property | Default | Meaning |
|----------|---------|---------|
| `TotalRuns` | `1` | How many complete executions of the scenario to perform |
| `MinSuccessRate` | `1.0` | Minimum fraction of runs that must pass (e.g. `0.8` = 80%) |

> A run "passes" when all CHECK assertions succeed for that execution.

## Basic Example

```csharp
var options = new ScenarioRunOptions
{
    TotalRuns = 3,
    MinSuccessRate = 0.67 // At least 2 of 3 must pass
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
| Deterministic (low temperature) | `TotalRuns = 1` | Fast feedback |
| Creative copy / marketing text | `TotalRuns = 5`, `MinSuccessRate = 0.6` | Allow variation, catch systemic issues |
| Tool / Function invocation | `TotalRuns = 3`, `MinSuccessRate = 0.67` | Ensure reliability of structured calls |
| Regression CI (critical path) | `TotalRuns = 5`, `MinSuccessRate = 0.8+` | High confidence before shipping |

## Pattern: Progressive Hardening

Start strict (require 100%) while iterating. If you observe rare, benign variations, *lower* only `MinSuccessRate` slightly instead of removing assertions.

```csharp
// Phase 1 (authoring): require perfection
new ScenarioRunOptions { TotalRuns = 1, MinSuccessRate = 1.0 };

// Phase 2 (stabilizing): allow one-off drift
new ScenarioRunOptions { TotalRuns = 3, MinSuccessRate = 0.67 };

// Phase 3 (mature): statistically strong signal
new ScenarioRunOptions { TotalRuns = 5, MinSuccessRate = 0.8 };
```

## Interpreting Failures

Failure message example:
```
Only 40% of rounds passed, which is below the required success rate of 80%
```
Indicates the model produced unacceptable answers in a *repeatable pattern* (not just noise) – investigate prompt, model, or assertions.

## Best Practices

- Keep `TotalRuns` small locally; increase in CI if needed
- Combine with **FunctionCall** and **JsonCheck** to enforce structure while allowing linguistic freedom
- Track historical pass rate drift to detect model regression
- Use higher thresholds for safety-sensitive outputs

---
**TL;DR:** `ScenarioRunOptions` adds statistical robustness to semantic tests, reducing flakes while still catching real regressions.
