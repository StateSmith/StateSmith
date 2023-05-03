#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace StateSmith.Input.DrawIo;

public class MxCellsToSmDiagramConverter
{
    public Dictionary<string, DiagramNode> nodeMap = new();
    public List<DiagramEdge> edges = new();
    public List<DiagramNode> roots = new();
    private readonly VisualGroupingValidator visualGroupingValidator;

    public MxCellsToSmDiagramConverter(VisualGroupingValidator visualGroupingValidator)
    {
        this.visualGroupingValidator = visualGroupingValidator;
    }

    public void Process(Dictionary<string, MxCell> mxCells)
    {
        IEnumerable<MxCell> verticeCells = GetVerticeCells(mxCells);

        foreach (var cell in verticeCells)
        {
            CreateAndAddVertex(cell);
        }

        foreach (var cell in verticeCells)
        {
            LinkVertexForCell(cell);
        }

        foreach (var cell in mxCells.Values.Where(c => c.type == MxCell.Type.Edge))
        {
            ProcessEdge(cell);
        }

        visualGroupingValidator.Process(mxCells, roots);
    }

    private static List<MxCell> GetVerticeCells(Dictionary<string, MxCell> mxCells)
    {
        var cells = mxCells.Values.Where(c => c.type == MxCell.Type.Vertex).ToList();

        for (int i = 0; i < cells.Count; )
        {
            var c = cells[i];
            if (ProcessFancyEdgeLabel(c, mxCells)) // can't put in above linq query because it seems to be executed multiple times
            {
                cells.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }

        return cells;
    }

    private static bool ProcessFancyEdgeLabel(MxCell c, Dictionary<string, MxCell> mxCells)
    {
        if (c.parent == null)
            return false;

        if (mxCells.TryGetValue(c.parent, out var parentCell))
        {
            if (parentCell.type == MxCell.Type.Edge)
            {
                parentCell.label = parentCell.label ?? "";
                parentCell.label += c.label;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// At this point, all vertices have been added to map.
    /// </summary>
    /// <param name="cell"></param>
    private void LinkVertexForCell(MxCell cell)
    {
        if (IsDrawIoRootCell(cell.id))
        {
            return;
        }

        if (IsInnerHandlerText(cell))
        {
            AddInnerHanlderTextToParent(cell);
        }
        else
        {
            var node = nodeMap[cell.id];
            node.parent = GetParentNode(cell.parent);

            if (node.parent == null)
            {
                roots.Add(node);
            }
            else
            {
                node.parent.children.Add(node);
            }
        }
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/191
    /// </summary>
    private void AddInnerHanlderTextToParent(MxCell cell)
    {
        string label = (cell.label ?? "").Trim();

        if (label.Length == 0)
        {
            return;
        }

        if (cell.parent == null)
        {
            throw new DrawIoException("InnerHandlerText found with a null parent. https://github.com/StateSmith/StateSmith/issues/191 .");
        }
        var parent = nodeMap[cell.parent];
        parent.label += "\n" + label;
        parent.subIds.Add(cell.id);
    }

    private DiagramNode? GetParentNode(string? parentId)
    {
        if (parentId == null)
        {
            throw new DrawIoException("unexpected null parent");
        }

        if (IsDrawIoRootCell(parentId))
        {
            return null;
        }

        return nodeMap[parentId];
    }

    // special draw.io root like cells have no type
    // <mxCell id="0" />
    // <mxCell id="1" parent="0" />
    private static bool IsDrawIoRootCell(string id)
    {
        return id == "0" || id == "1";
    }

    private void CreateAndAddVertex(MxCell cell)
    {
        // ignore special root like draw.io cells
        if (IsDrawIoRootCell(cell.id))
        {
            return;
        }

        if (IsInnerHandlerText(cell))
        {
            return;
        }

        var node = new DiagramNode
        {
            id = cell.id,
            label = cell.label ?? ""
        };
        nodeMap.Add(node.id, node);
    }

    /// <summary>
    /// Starting with draw.io plugin support, we allow an MxCell to have its event handlers specified in its label
    /// and also inside it.
    /// https://github.com/StateSmith/StateSmith/issues/191
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    internal static bool IsInnerHandlerText(MxCell cell)
    {
        if (cell.StyleMap.Count == 0)
            return false;

        var matches = cell.HasMatchingStyle("fillColor", "none") &&
                      cell.HasMatchingStyle("gradientColor", "none") &&
                      cell.HasMatchingStyle("strokeColor", "none") &&
                      cell.HasShape() == false;

        return matches;
    }

    private void ProcessEdge(MxCell cell)
    {
        if (cell.target == null)
        {
            throw new DrawIoException($"Unterminated edge (no target). Edge diagram id:`{cell.id}`, label:`{cell.label}`");
        }

        if (cell.source == null)
        {
            throw new DrawIoException($"Edge without source. Edge diagram id:`{cell.id}`, label:`{cell.label}`");
        }

        var edge = new DiagramEdge
        (
            id: cell.id,
            label: cell.label ?? "",
            target: nodeMap[cell.target],
            source: nodeMap[cell.source]
        );
        edges.Add(edge);
    }
}
