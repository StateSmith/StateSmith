using StateSmith.Runner;
using System.Collections.Generic;
using System.IO;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace StateSmith.Input.DrawIo;

public class DrawIoToSmDiagramConverter
{
    public List<DiagramEdge> Edges = new();
    public List<DiagramNode> Roots = new();
    private readonly IConsolePrinter consolePrinter;

    public IServiceProvider serviceProvider { get; }

    public DrawIoToSmDiagramConverter(IServiceProvider serviceProvider, IConsolePrinter consolePrinter)
    {
        this.serviceProvider = serviceProvider;
        this.consolePrinter = consolePrinter;
    }

    public void ProcessFile(string filePath)
    {
        Console.WriteLine($"BOOGA DrawIoToSmDiagramConverter.ProcessFile({filePath})");
        // throw new ArgumentException();
        using StreamReader fileReader = File.OpenText(filePath);

        if (filePath.EndsWith(".svg", System.StringComparison.OrdinalIgnoreCase))
        {
            ProcessSvg(fileReader);
        }
        else
        {
            ProcessRegularFile(fileReader);
        }
    }

    public void ProcessSvg(TextReader svgFileReader)
    {
        var diagrams = DrawIoDecoder.DecodeSvgToOriginalDiagrams(svgFileReader);
        ProcessDiagrams(diagrams);
    }

    private void ProcessDiagrams(List<DrawIoDiagramNode> diagrams)
    {
        foreach (var d in diagrams)
        {
            if (!IsNotesPage(d))
            {
                ProcessDiagramContents(d.xml);
            }
        }
    }

    private static bool IsNotesPage(DrawIoDiagramNode d)
    {
        return d.name.Trim().StartsWith("$notes", System.StringComparison.OrdinalIgnoreCase);
    }

    public void ProcessRegularFile(TextReader fileReader)
    {
        var diagrams = DrawIoDecoder.GetMxFileDiagramContents(fileReader.ReadToEnd());
        ProcessDiagrams(diagrams);
    }

    public void ProcessDiagramContents(string diagramXml)
    {
        MxCellParser mxCellParser = new(diagramXml, consolePrinter);
        mxCellParser.Parse();

        // we need a new converter for each diagram
        var converter = serviceProvider.GetRequiredService<MxCellsToSmDiagramConverter>();
        converter.Process(mxCellParser.mxCells);

        Edges.AddRange(converter.edges);
        Roots.AddRange(converter.roots);
    }
}
