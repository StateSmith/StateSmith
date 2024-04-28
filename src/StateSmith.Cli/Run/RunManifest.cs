using System.Collections.Generic;

namespace StateSmith.Cli.Run;

public class ProjectSetting
{
    public string CsxPath { get; set; } = "";

    /// <summary>
    /// A csx script can generate multiple state machines.
    /// This is discovered when running the csx file. 
    /// The csx script will print a message like: `StateSmith Runner - Compiling file: `LightSm.drawio.svg` (no state machine name specified).`
    /// </summary>
    public List<string> DiagramPaths { get; set; } = new();

    /// <summary>
    /// Ignores the last run timestamp and always builds the project.
    /// If either csx or diagram file has been modified, a new build is required.
    /// </summary>
    public bool AlwaysBuild { get; set; } = false;
}


public class RunManifest
{
    public List<ProjectSetting> DiscoveredProjects { get; set; } = new();

    public List<ProjectSetting> ManuallySpecifiedProjects { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    public List<string> PathsToIgnoreForDiscovery { get; set; } = new();
}


/// <summary>
/// Lives beside nuget and other settings files. Not in the project directory.
/// We don't use the modified timestamp of the generated files because the csx file may change file extensions/paths.
/// Using git, they might not be accurate either.
/// If git committed a changed csx file, but code gen wasn't run, then the generated files will be "dirty", but still have the same modified timestamp.
/// This would make it look like the generated files are up to date, when they are not.
/// </summary>
public class RunData
{
    //modified timestamp for last run
}

/*

ss.cli run --here



ss.cli run

    > no statesmith manifest found. What do you want to do?
    >> automatically create a manifest file here
    >> search up the directory tree for manifest and run that (limit of X directories)
    >> find and run StateSmith csx files in this directory
    >> exit


statesmith.manifest.json
{
    DiscoveredProjects: [
        {
            CsxPath: "./path/to/project/RocketSm.csx",
            DiagramPath: "./path/to/project/RocketSm.csx",
            AlwaysBuild: false
        }
    ],
}



ss.cli run

> reading statesmith.manifest.json
> found 3 project to build
> project 1 of 3: C:\path\to\project.csx
> project is already up to date. skipping.
> project 2 of 3: C:\path\to\project2.csx
> project needs to be built. building.



ss.cli run --discover --recursive
> respects the `PathsToIgnoreForDiscovery` list
> updates the manifest file with the new projects found

ss.cli run --force-build
> forces a rebuild of all projects


*/
