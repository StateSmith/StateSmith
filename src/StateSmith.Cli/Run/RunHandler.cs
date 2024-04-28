using StateSmithTest.Processes;
using System;
using System.Collections.Generic;
using System.IO;

namespace StateSmith.Cli.Run;

public class RunHandler
{
    private CsxOutputParser parser;

    List<CsxRunInfo> runInfos = new();

    public RunHandler()
    {
        parser = new CsxOutputParser();
    }

    public void Run(string searchDirectory, bool recursive = false)
    {
        SsCsxFileFinder finder = new();
        if (recursive)
            finder.SetAsRecursive();

        var csxScripts = finder.Scan(searchDirectory: searchDirectory);

        foreach (var csx in csxScripts)
        {
            Console.WriteLine($"Running script {csx}");

            string csxPath = $"{searchDirectory}/{csx}";
            SimpleProcess process = new()
            {
                WorkingDirectory = Environment.CurrentDirectory,
                SpecificCommand = "dotnet-script",
                SpecificArgs = csxPath,
                throwOnExitCode = true
            };
            process.EnableEchoToTerminal();
            process.Run(timeoutMs: 6000);

            csxPath = Path.GetFullPath(csxPath);

            var info = new CsxRunInfo(csxPath: csxPath);
            parser.ParseAndResolveFilePaths(process.StdOutput, info);
            runInfos.Add(info);
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
