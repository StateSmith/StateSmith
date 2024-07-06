using System;
using System.Collections.Generic;
using System.IO;

namespace StateSmith.Runner;

public class DiagramFileAssociator
{
    private const string DrawioSvgExtension = ".drawio.svg";
    protected static readonly HashSet<string> PlantUmlFileExtensions = new() { ".pu", ".puml", ".plantuml" };
    protected static readonly HashSet<string> YedFileExtensions = new() { ".graphml" };
    protected static readonly HashSet<string> DrawIoFileEndings = new() { DrawioSvgExtension, ".drawio", ".dio" };

    public static List<string> GetAllDiagramExtensions()
    {
        var allExtensions = new List<string>();
        allExtensions.AddRange(DrawIoFileEndings);
        allExtensions.AddRange(PlantUmlFileExtensions);
        allExtensions.AddRange(YedFileExtensions);
        return allExtensions;
    }

    public bool IsPlantUmlExtension(string lowerCaseFileExtension)
    {
        return PlantUmlFileExtensions.Contains(lowerCaseFileExtension);
    }

    public bool IsPlantUmlFile(string filePath)
    {
        return IsPlantUmlExtension(Path.GetExtension(filePath));
    }

    public bool IsYedExtension(string lowerCaseFileExtension)
    {
        return YedFileExtensions.Contains(lowerCaseFileExtension);
    }

    public static bool IsDrawIoSvgFile(string filePath)
    {
        return filePath.EndsWith(DrawioSvgExtension, StringComparison.OrdinalIgnoreCase);
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
