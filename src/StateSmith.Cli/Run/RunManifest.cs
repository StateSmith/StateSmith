using System.Collections.Generic;

namespace StateSmith.Cli.Run;

public class RunManifest
{
    //public List<ProjectSetting> ManuallySpecifiedProjects { get; set; } = new();

    public List<string> IncludePathGlobs { get; set; } = new();
    public List<string> ExcludePathGlobs { get; set; } = new();
}
