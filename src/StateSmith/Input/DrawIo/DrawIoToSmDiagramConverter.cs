using System.Collections.Generic;
using System.IO;

namespace StateSmith.Input.DrawIo;

public class DrawIoToSmDiagramConverter
{
    private readonly MxCellsToSmDiagramConverter converter;

    public List<DiagramEdge> Edges => converter.edges;
    public List<DiagramNode> Roots => converter.roots;

    public DrawIoToSmDiagramConverter(MxCellsToSmDiagramConverter converter)
    {
        this.converter = converter;
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
        var diagramXml = DrawIoDecoder.DecodeSvgToOriginalDiagram(svgFileReader);
        ProcessDiagramContents(diagramXml);
    }

    public void ProcessRegularFile(TextReader fileReader)
    {
        var diagramXml = DrawIoDecoder.GetMxFileDiagramContents(fileReader.ReadToEnd());
        ProcessDiagramContents(diagramXml);
    }

    public void ProcessDiagramContents(string diagramXml)
    {
        MxCellParser mxCellParser = new(diagramXml);
        mxCellParser.Parse();

        converter.Process(mxCellParser.mxCells);
    }
}
