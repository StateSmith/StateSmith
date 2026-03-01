#nullable enable

using StateSmith.Common;
using StateSmith.Output;
using System;
using System.Collections.Generic;
using System.IO;

namespace StateSmithTest.Output;

/// <summary>
/// Captures what would normally be written to file to memory instead.
/// </summary>
public class CapturingCodeFileWriter : ICodeFileWriter
{
    /// <summary>
    /// indexed by ABSOLUTE filepath (key)
    /// </summary>
    public HashList<string, Capture> captures = new();
    public Capture? lastCapture = null;
    public string LastCode => lastCapture?.code ?? "";
    public int captureCount = 0;

    public void WriteFile(string filePath, string code)
    {
        Capture capture = new(filePath, code);
        lastCapture = capture;
        captures.Add(filePath, capture);
        captureCount++;
    }

    /// <summary>
    /// Finds captures for a file name (not a file path!)
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public List<Capture> GetCapturesForFileName(string fileName)
    {
        List<Capture> matchedCaptures = [];
        foreach (var absoluteFilePath in captures.GetKeys())
        {
            if (Path.GetFileName(absoluteFilePath) == fileName)
            {
                matchedCaptures.AddRange(captures.GetValues(absoluteFilePath));
            }
        }

        return matchedCaptures;
    }

    public Capture GetSoleCaptureWithName(string fileName)
    {
        var fileNameCaptures = GetCapturesForFileName(fileName);
        if (fileNameCaptures.Count != 1)
        {
            throw new System.Exception($"Expected 1 capture for {fileName}, but found {fileNameCaptures.Count}. File names captured: {string.Join(",", captures.GetKeys())}");
        }

        return fileNameCaptures[0];
    }

    public List<Capture> GetCapturesEndingWith(string fileEnding)
    {
        return GetMatchingCaptures((capture) => capture.filePath.EndsWith(fileEnding));
    }

    public List<Capture> GetMatchingCaptures(Predicate<Capture> matcher)
    {
        List<Capture> matchedCaptures = [];

        foreach (var capture in captures.GetAllValues())
        {
            if (matcher(capture))
            {
                matchedCaptures.Add(capture);
            }
        }

        return matchedCaptures;
    }

    public class Capture
    {
        public string filePath;
        public string code;

        public Capture(string filePath, string code)
        {
            this.filePath = filePath;
            this.code = code;
        }
    }
}
