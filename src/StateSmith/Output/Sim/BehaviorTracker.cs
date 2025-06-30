#nullable enable

using StateSmith.SmGraph;
using System;
using System.Collections.Generic;

namespace StateSmith.Output.Sim;

/// <summary>
/// This class is used to track the original UML representation of a behavior before it is modified to be used in the simulator.
/// https://github.com/StateSmith/StateSmith/issues/381
/// </summary>
public class BehaviorTracker
{
    Dictionary<Behavior, string> mappingToOriginalUml = new();

    public void RecordOriginalBehavior(Behavior behavior)
    {
        if (mappingToOriginalUml.ContainsKey(behavior))
        {
            throw new InvalidOperationException("Behavior already added");
        }
        mappingToOriginalUml[behavior] = behavior.DescribeAsUml(singleLineFormat:false);
    }

    /// <summary>
    /// Tries to get the original UML representation of a behavior. If it is not found, it will return the current UML representation.
    /// </summary>
    /// <param name="behavior"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public string GetOriginalUmlOrCurrent(Behavior behavior)
    {
        if (mappingToOriginalUml.TryGetValue(behavior, out string? originalUml))
        {
            return originalUml;
        }
        return behavior.DescribeAsUml(singleLineFormat: false);
    }
}
