#nullable enable

using StateSmith.Common;
using StateSmith.Output;
using System;

namespace StateSmith.Input.Timers;

public class TimerExpander
{
    readonly TimerExpanderSettings settings;
    readonly IExpansionVarsPathProvider expansionVarsPathProvider;

    public TimerExpander(IExpansionVarsPathProvider expansionVarsPathProvider, TimerExpanderSettings settings)
    {
        this.expansionVarsPathProvider = expansionVarsPathProvider;
        this.settings = settings;
    }

    /// <summary>
    /// This is a local variable defined in event function handler.
    /// </summary>
    private const string TIME_IN_STATE_VAR_NAME = "_time_in_state";

    private string TimeAccessCode => settings.timeAccessCode.ThrowIfNull("FIXME - provide helpful user error message");

    public virtual string TryExpandVariableExpansion(string name)
    {
        int level = 0;  // FIXME

        switch (name)
        {
            case "$tis":
            case "$time_in_state": return TIME_IN_STATE_VAR_NAME;
            case "$state_entered_time": return $"{expansionVarsPathProvider.ExpansionVarsPath}_timer_level_{level}_start";
            case "$time": return TimeAccessCode;
        }

        return name;
    }

    public virtual string TryExpandFunction(string name)
    {
        return name; // FIXME
    }

    internal bool HasFunctionName(string functionName)
    {
        throw new NotImplementedException();
    }

    internal string TryExpandFunctionExpansion(string functionName, string[] strings)
    {
        throw new NotImplementedException();
    }
}
