using StateSmith.Cli.Run;

namespace StateSmith.Cli;

public class Manifest
{
    public ManifestVersion Version { get; set; } = new(0, 1, 0);
    public RunManifest RunManifest { get; set; } = new();

    public class ManifestVersion
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public string tag { get; set; } = "";

        public ManifestVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }
    }
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
