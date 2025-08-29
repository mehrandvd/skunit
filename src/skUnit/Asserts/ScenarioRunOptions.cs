using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skUnit;

/// <summary>
/// Controls multi-run execution and pass criteria for scenario assertions.
/// </summary>
public class ScenarioRunOptions
{
    /// <summary>
    /// Number of complete scenario executions to perform. Must be >= 1.
    /// </summary>
    public int TotalRuns { get; set; } = 1;

    /// <summary>
    /// Minimum fraction (0–1] of runs that must pass. Default 1.0 (all runs must pass).
    /// Passes when (PassedRuns / TotalRuns) >= MinSuccessRate.
    /// </summary>
    public double MinSuccessRate { get; set; } = 1.0;
}
