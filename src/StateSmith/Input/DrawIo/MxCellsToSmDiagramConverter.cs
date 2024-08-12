#nullable enable

using StateSmith.SmGraph;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace StateSmith.Input.DrawIo;

public class MxCellsToSmDiagramConverter
{
    public Dictionary<string, DiagramNode> nodeMap = new();  
    public List<DiagramEdge> edges = new();
    public List<DiagramNode> roots = new();
    private readonly VisualGroupingValidator visualGroupingValidator;
    private Dictionary<string, MxCell> mxCellMap = new();

    public MxCellsToSmDiagramConverter(VisualGroupingValidator visualGroupingValidator)
    {
        this.visualGroupingValidator = visualGroupingValidator;
    }

    public void Process(Dictionary<string, MxCell> mxCellsMap)
    {
        mxCellMap = mxCellsMap;
        IEnumerable<MxCell> verticeCells = GetVerticeCells(mxCellsMap);

        foreach (var cell in verticeCells)
        {
            CreateAndAddVertex(cell);
        }

        foreach (var cell in verticeCells)
        {
            LinkVertexForCell(cell);
        }

        foreach (var cell in mxCellsMap.Values.Where(c => c.type == MxCell.Type.Edge))
        {
            ProcessEdge(cell);
        }

        visualGroupingValidator.Process(mxCellsMap, roots);
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
    private bool IsDrawIoRootCell(string id)
    {
        // Note! we can't always rely on `id == "0" || id == "1"`. It is sometimes different (especially for additional pages).
        return mxCellMap[id].isSpecialInvisbleRootTypeNode;
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

        // https://github.com/StateSmith/StateSmith/issues/192
        if (IsNoteShape(cell))
        {
            node.label = VertexParseStrings.Notes + "\n" + node.label;
        }

        nodeMap.Add(node.id, node);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/192
    /// </summary>
    private static bool IsNoteShape(MxCell cell)
    {
        var shape = cell.GetStyleFor("shape");

        if (shape == null)
            return false;

        switch (shape)
        {
            case "note":
            case "note2":
            case "mxgraph.mockup.text.stickyNote2":
                return true;
        }

        return false;
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
            var msg = $"Unterminated edge/arrow (no target connection).";
            ThrowBadEdgeException(cell, msg);
        }

        if (cell.source == null)
        {
            var msg = $"Edge/arrow not connected at source.";
            ThrowBadEdgeException(cell, msg);
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

    [DoesNotReturn]
    private void ThrowBadEdgeException(MxCell cell, string msg)
    {
        var msg2 = msg + "\nMake sure transition edge/arrow is properly connected to shapes (not left dangling).\n";
        msg2 += GetEdgeDebugDetails(cell);
        throw new DrawIoException(msg2);
    }

    public string GetEdgeDebugDetails(MxCell cell)
    {
        StringBuilder sb = new();
        sb.Append("\nBAD EDGE INFO\n");
        sb.Append($"diagram id: `{cell.id}`\nlabel:`{cell.label}`\n");

        sb.Append("============================\n");
        sb.Append("EDGE SOURCE NODE\n");
        NewMethod(sb, cell.source);

        sb.Append("==========================\n");
        sb.Append("EDGE TARGET NODE\n");
        NewMethod(sb, cell.target);

        // cell parent is often null or root so we handle it separately
        if (cell.parent != null && nodeMap.ContainsKey(cell.parent))
        {
            sb.Append("============================\n");
            sb.Append("PARENT NODE\n");
            nodeMap[cell.parent].Describe(sb);
            sb.Append('\n');
        }

        return sb.ToString();
    }

    private void NewMethod(StringBuilder sb, string? id)
    {
        if (id == null)
        {
            sb.Append("null (you need to connect this)\n");
        }
        else
        {
            // just in case the node is missing
            try
            {
                nodeMap[id].Describe(sb);
                sb.Append('\n');
            }
            catch
            {
                sb.Append($"Failed looking up id: `{id}`\n");
            }
        }
    }
}
