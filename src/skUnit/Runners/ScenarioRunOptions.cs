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
    /// Number of successful runs required for the scenario to pass. When null, all runs must pass.
    /// </summary>
    public int? RequiredSuccessRuns { get; set; }
}
