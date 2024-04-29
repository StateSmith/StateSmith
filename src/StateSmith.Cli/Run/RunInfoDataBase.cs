using StateSmith.Cli.Utils;
using System;
using System.IO;
using System.Security.Cryptography;

namespace StateSmith.Cli.Run;

public class RunInfoDataBase
{
    protected string _tempPath = Path.GetTempPath() + "/statesmith";
    string dirOrManifestPath;

    public RunInfoDataBase(string dirOrManifestPath)
    {
        this.dirOrManifestPath = dirOrManifestPath;
    }

    public RunInfo? ReadRunInfoDatabase()
    {
        RunInfo? readRunInfo = null;

        string runInfoFilePath = GetRunInfoFilePath(dirOrManifestPath);
        if (!File.Exists(runInfoFilePath))
        {
            Console.WriteLine("No run info found at: " + runInfoFilePath);
        }
        else
        {
            Console.WriteLine("Reading run info from: " + runInfoFilePath);
            Console.Out.Flush(); //flush to ensure that the message is printed incase of a crash/exception

            JsonFilePersistence jsonFilePersistence = new()
            {
                IncludeFields = true
            };

            try
            {
                readRunInfo = jsonFilePersistence.RestoreFromFile<RunInfo>(runInfoFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading run info: " + e.Message);
            }
        }

        return readRunInfo;
    }

    public void PersistRunInfo(RunInfo runInfo)
    {
        //create temp directory if it doesn't exist
        if (!Directory.Exists(_tempPath))
        {
            Directory.CreateDirectory(_tempPath);
        }

        string runInfoFilePath = GetRunInfoFilePath(dirOrManifestPath);

        JsonFilePersistence jsonFilePersistence = new()
        {
            IncludeFields = true
        };
        jsonFilePersistence.PersistToFile(runInfo, runInfoFilePath);
        Console.WriteLine("Run info stored in " + runInfoFilePath);
    }

    private string GetRunInfoFilePath(string targetDirOrFile)
    {
        targetDirOrFile = Path.GetFullPath(targetDirOrFile);
        MD5 md5 = MD5.Create();
        var bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(targetDirOrFile));
        var tempFileName = $"{_tempPath}/runinfo.{BitConverter.ToString(bytes).Replace("-", "").ToLower()}.json";
        return tempFileName;
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
