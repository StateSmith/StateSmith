using StateSmith.Runner;
using System.Collections.Generic;
using System.IO;

namespace StateSmith.Input.DrawIo;

public class DrawIoToSmDiagramConverter
{
    public List<DiagramEdge> Edges = new();
    public List<DiagramNode> Roots = new();

    public DiServiceProvider DiServiceProvider { get; }

    public DrawIoToSmDiagramConverter(DiServiceProvider diServiceProvider)
    {
        DiServiceProvider = diServiceProvider;
    }

    public void ProcessFile(string filePath)
    {
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
        MxCellParser mxCellParser = new(diagramXml);
        mxCellParser.Parse();

        // we need a new converter for each diagram
        var converter = DiServiceProvider.GetInstanceOf<MxCellsToSmDiagramConverter>();
        converter.Process(mxCellParser.mxCells);

        Edges.AddRange(converter.edges);
        Roots.AddRange(converter.roots);
    }
}
