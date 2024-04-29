using System.Collections.Generic;

namespace StateSmith.Cli.Run;

public class ProjectSetting
{
    public string CsxRelativePath { get; set; } = "";

    /// <summary>
    /// Ignores the last run timestamp and always builds the project.
    /// If either csx or diagram file has been modified, a new build is required.
    /// </summary>
    public bool AlwaysBuild { get; set; } = false;

    public ProjectSetting(string csxRelativePath)
    {
        CsxRelativePath = csxRelativePath;
    }
}


public class RunManifest
{
    public List<ProjectSetting> AutoDiscoveredProjects { get; set; } = new();

    //public List<ProjectSetting> ManuallySpecifiedProjects { get; set; } = new();
    public List<string> PathGlobsToIgnoreForDiscovery { get; set; } = new();
    public List<string> PathGlobsForDiscovery { get; set; } = new();
}



/*

# run with manifest
ss.cli run [--path] [--rebuild]
    path to manifest file or directory

    no statesmith manifest found. What do you want to do?
    >> automatically create a manifest file here
    >> search up the directory tree for manifest and run that (limit of X directories)
    >> find and run StateSmith csx files in this directory
    >> exit

# search up for manifest
ss.cli run -u [--rebuild]

# show choices
ss.cli run --choose

# run without manifest
ss.cli run-csx [--rebuild] [--recursive] [--paths *.csx] [--exclude-paths]
    
    
 
 */
