#nullable enable

using StateSmith.Output.Algos.Balanced1;
using StateSmith.Output.Algos.Balanced2;

namespace StateSmith.Runner;

/// <summary>
/// https://github.com/StateSmith/StateSmith/wiki/Algorithms
/// </summary>
public enum AlgorithmId
{
    Default,

    /// <summary>
    /// Previous default. See <see cref="AlgoBalanced1"/>, <see cref="AlgoBalanced1Settings"/>, <see cref="AlgoStateIdToString"/>, <see cref="AlgoEventIdToString"/>
    /// </summary>
    Balanced1,

    /// <summary>
    /// Default for now. Derived from <see cref="Balanced1"/>. See <see cref="AlgoBalanced2"/>, <see cref="AlgoBalanced1Settings"/>, <see cref="AlgoStateIdToString"/>, <see cref="AlgoEventIdToString"/>
    /// </summary>
    Balanced2,
}


