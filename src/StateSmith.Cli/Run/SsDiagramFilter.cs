using StateSmith.Runner;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace StateSmith.Cli.Run;

/// <summary>
/// Determines if a file is a StateSmith diagram file.
/// </summary>
public class SsDiagramFilter
{
    DiagramFileAssociator fileAssociator;

    public SsDiagramFilter(DiagramFileAssociator fileAssociator)
    {
        this.fileAssociator = fileAssociator;
    }

    public bool IsTargetDiagramFile(string path, string fileText)
    {
        string lowerCaseFileExtension = Path.GetExtension(path).ToLower();

        if (fileAssociator.IsDrawIoFile(path) || fileAssociator.IsYedExtension(lowerCaseFileExtension))
        {
            return IsSsDrawioYedFileContents(fileText);
        }
        else if (fileAssociator.IsPlantUmlExtension(lowerCaseFileExtension))
        {
            return IsSsPlantUmlFileContents(fileText);
        }

        return false;
    }

    /// <summary>
    /// Both formats have a $STATEMACHINE string in them.
    /// .drawio.svg has encoded/compressed xml contents, but the svg should still have the $STATEMACHINE string.
    /// </summary>
    /// <param name="diagramContents"></param>
    /// <returns></returns>
    public bool IsSsDrawioYedFileContents(string diagramContents)
    {
        return diagramContents.Contains("$STATEMACHINE");
    }

    public bool IsSsPlantUmlFileContents(string diagramContents)
    {
        return diagramContents.Contains("@startuml");
    }
}
