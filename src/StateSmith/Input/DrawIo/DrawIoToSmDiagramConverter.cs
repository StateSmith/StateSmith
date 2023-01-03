using System.Collections.Generic;
using System.IO;

namespace StateSmith.Input.DrawIo;

public class DrawIoToSmDiagramConverter
{
    readonly MxCellsToSmDiagramConverter converter = new();

    public List<DiagramEdge> Edges => converter.edges;
    public List<DiagramNode> Roots => converter.roots;

    public void ProcessFile(string filePath)
    {
        StreamReader fileReader = File.OpenText(filePath);

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
        var diagramXml = DrawIoSvgDecoder.DecodeToOriginalDiagram(svgFileReader);
        ProcessRegularFile(new StringReader(diagramXml));
    }

    public void ProcessRegularFile(TextReader fileReader)
    {
        MxCellParser mxCellParser = new(fileReader);
        mxCellParser.Parse();

        converter.Process(mxCellParser.mxCells);
    }
}
