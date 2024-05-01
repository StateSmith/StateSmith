using StateSmith.Cli.Utils;
using System;
using System.IO;
using System.Security.Cryptography;

namespace StateSmith.Cli.Run;

public class RunInfoDataBase
{
    protected string _tempPath = Path.GetTempPath() + "StateSmith.Cli";
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
        var tempFileName = $"{_tempPath}{Path.DirectorySeparatorChar}runinfo.{BitConverter.ToString(bytes).Replace("-", "").ToLower()}.json";
        return tempFileName;
    }
}
