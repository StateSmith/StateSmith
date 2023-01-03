using System;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.Input.DrawIo;

public class MxCellsToSmDiagramConverter
{
    public Dictionary<string, DiagramNode> nodeMap = new();
    public List<DiagramEdge> edges = new();
    public List<DiagramNode> roots = new();

    public void Process(Dictionary<string, MxCell> mxcells)
    {
        IEnumerable<MxCell> verticeCells = mxcells.Values.Where(c => c.type == MxCell.Type.Vertex);

        foreach (var cell in verticeCells)
        {
            CreateAndAddVertex(cell);
        }

        foreach (var cell in verticeCells)
        {
            LinkVertexForCell(cell);
        }

        foreach (var cell in mxcells.Values.Where(c => c.type == MxCell.Type.Edge))
        {
            ProcessEdge(cell);
        }
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

    private DiagramNode GetParentNode(string parentId)
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

        var node = new DiagramNode
        {
            id = cell.id,
            label = cell.label
        };
        nodeMap.Add(node.id, node);
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
        {
            id = cell.id,
            label = cell.label,
            target = nodeMap[cell.target],
            source = nodeMap[cell.source]
        };
        edges.Add(edge);
    }
}
