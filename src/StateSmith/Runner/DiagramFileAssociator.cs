using System;
using System.Collections.Generic;
using System.IO;

namespace StateSmith.Runner;

public static class DiagramFileAssociator
{
    private const string DrawioSvgExtension = ".drawio.svg";
    private static readonly HashSet<string> PlantUmlFileExtensions = new() { ".pu", ".puml", ".plantuml" };
    private static readonly HashSet<string> YedFileExtensions = new() { ".graphml" };
    private static readonly HashSet<string> DrawIoFileEndings = new() { DrawioSvgExtension, ".drawio", ".dio" };

    public static List<string> GetAllDiagramExtensions()
    {
        var allExtensions = new List<string>();
        allExtensions.AddRange(DrawIoFileEndings);
        allExtensions.AddRange(PlantUmlFileExtensions);
        allExtensions.AddRange(YedFileExtensions);
        return allExtensions;
    }

    public static bool IsPlantUmlExtension(string lowerCaseFileExtension)
    {
        return PlantUmlFileExtensions.Contains(lowerCaseFileExtension);
    }

    public static bool IsPlantUmlFile(string filePath)
    {
        return IsPlantUmlExtension(Path.GetExtension(filePath));
    }

    public static bool IsYedFile(string filePath)
    {
        return IsYedExtension(Path.GetExtension(filePath).ToLower());
    }

    public static bool IsYedExtension(string lowerCaseFileExtension)
    {
        return YedFileExtensions.Contains(lowerCaseFileExtension);
    }

    public static bool IsDrawIoSvgFile(string filePath)
    {
        return filePath.EndsWith(DrawioSvgExtension, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsDrawIoFile(string filePath)
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

    public static string GetHelpMessage()
    {
        var helpMessage = 
                  $"  - draw.io supports: {string.Join(", ", DrawIoFileEndings)}\n"
                + $"  - yEd supports: {string.Join(", ", YedFileExtensions)}\n"
                + $"  - PlantUML supports: {string.Join(", ", PlantUmlFileExtensions)}\n";
        return helpMessage;
    }
}
