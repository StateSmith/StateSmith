using System;
using System.IO;

namespace StateSmith.Cli;

public class ConfigDataPath
{
    public string GetConfigDirPath()
    {
        string dirPath;

        bool local = false;

        if (local)
        {
            // local is a bit more for cache, temp, etc.

            // "C:\\Users\\user\\AppData\\Local" - specific to the user, stays on the machine
            // "/home/user/.local/share"
            dirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
        else
        {
            // "C:\\Users\\user\\AppData\\Roaming" - specific to the user, roams with the user (other machines with same user)
            // "/home/user/.config"
            dirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        // create directory if it doesn't exist
        dirPath = Path.Combine(dirPath, "StateSmith.Cli");
        Directory.CreateDirectory(dirPath);

        return dirPath;
    }
}
