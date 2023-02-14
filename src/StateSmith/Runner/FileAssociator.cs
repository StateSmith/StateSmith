using System;
using System.Collections.Generic;

namespace StateSmith.Runner;

public class FileAssociator
{
    protected HashSet<string> PlantUmlFileExtensions = new() { ".pu", ".puml", ".plantuml" };
    protected HashSet<string> YedFileExtensions = new() { ".graphml" };
    protected HashSet<string> DrawIoFileEndings = new() { ".drawio.svg", ".drawio", ".dio" };

    public bool IsPlantUmlExtension(string lowerCaseFileExtension)
    {
        return PlantUmlFileExtensions.Contains(lowerCaseFileExtension);
    }

    public bool IsYedExtension(string lowerCaseFileExtension)
    {
        return YedFileExtensions.Contains(lowerCaseFileExtension);
    }

    public bool IsDrawIoFile(string filePath)
    {
        foreach (var matchingEnding in DrawIoFileEndings)
        {
            if (filePath.EndsWith(matchingEnding, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public string GetHelpMessage()
    {
        var helpMessage = 
                  $"  - draw.io supports: {string.Join(", ", DrawIoFileEndings)}\n"
                + $"  - yEd supports: {string.Join(", ", YedFileExtensions)}\n"
                + $"  - PlantUML supports: {string.Join(", ", PlantUmlFileExtensions)}\n";
        return helpMessage;
    }
}
