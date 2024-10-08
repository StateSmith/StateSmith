using System.Collections.Generic;

namespace StateSmith.Cli.Run;

public class SuccessTracker
{
    private HashSet<string> _failedSources = new();
    private HashSet<string> _successfulSources = new();

    public IReadOnlySet<string> FailedSources => _failedSources;
    public IReadOnlySet<string> SuccessfulSources => _successfulSources;

    public void AddFailure(string source)
    {
        _failedSources.Add(source);
        _successfulSources.Remove(source);
    }

    public void AddSuccess(string source)
    {
        _successfulSources.Add(source);
        _failedSources.Remove(source);
    }
}
