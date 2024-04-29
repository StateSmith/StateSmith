#nullable disable // This code is old. Needs updating or possibly removal.

using System;
using System.IO;
using System.Security.Cryptography;

namespace StateSmith.Cli.CompRun;

public class AssemblyCacher
{
    public string directoryPath = "./.statesmith/";

    private string hashString;

    public AssemblyCacher(string code)
    {
        hashString = Hash(code);
    }

    public static string Hash(string code)
    {
        // if SHA1 is good enough for git, good enough for us.
        // SHA1 outputs 160 bits, 20 bytes, 40 hex chars. SHA256 outputs 32 bytes, 64 hex chars which is may cause filename too long issues anyway.
        SHA1 shaHash = SHA1.Create();

        var stringBytes = System.Text.Encoding.UTF8.GetBytes(code);
        var hashBytes = shaHash.ComputeHash(stringBytes);
        var hexHash = BitConverter.ToString(hashBytes).Replace("-", "");    //normally separates hex bytes with dashes

        return hexHash;
    }

    public byte[] TryGetFromCache()
    {
        string filename = BuildFilePath(hashString);
        if (File.Exists(filename))
        {
            var bytes = File.ReadAllBytes(filename);
            return bytes;
        }

        return null;
    }

    public void Cache(byte[] bytes)
    {
        Directory.CreateDirectory(directoryPath);

        string filename = BuildFilePath(hashString);

        File.WriteAllBytes(filename, bytes);
    }

    private string BuildFilePath(string hashString)
    {
        return directoryPath + hashString;
    }
}
