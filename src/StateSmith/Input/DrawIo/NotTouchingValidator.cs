#nullable enable

using StateSmith.SmGraph;
using StateSmith.Input.Antlr4;
using System.Collections.Generic;

namespace StateSmith.Input.DrawIo;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/81
/// </summary>
public class NotTouchingValidator
{
    public const string HelpUrl = "https://github.com/StateSmith/StateSmith/issues/81";

    Dictionary<string, CornerPoints> cellIdToCornerPointsMapping;
    Dictionary<string, MxCell> mxCellMap;

    List<DiagramNode> nodesToCheckAgainst = new();

    /// <summary>
    /// When a group is collapsed, we don't care about checking its children against nodes outside of the group for collisions.
    /// </summary>
    Queue<DiagramNode> collisionRootsToCheck = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cellIdToCornerPointsMapping"></param>
    /// <param name="mxCellMap"></param>
    public NotTouchingValidator(Dictionary<string, CornerPoints> cellIdToCornerPointsMapping, Dictionary<string, MxCell> mxCellMap)
    {
        this.cellIdToCornerPointsMapping = cellIdToCornerPointsMapping;
        this.mxCellMap = mxCellMap;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="topLevelNodes"></param>
    public void ProcessTopLevelNodes(List<DiagramNode> topLevelNodes)
    {
        var dummyRoot = new DiagramNode();
        dummyRoot.children.AddRange(topLevelNodes);
        collisionRootsToCheck.Enqueue(dummyRoot);

        while (collisionRootsToCheck.Count > 0)
        {
            nodesToCheckAgainst.Clear();
            var collisionRoot = collisionRootsToCheck.Dequeue();

            foreach (var child in collisionRoot.children)
            {
                Process(child);
            }
        }
    }

    private void Process(DiagramNode node)
    {
        var myCorners = cellIdToCornerPointsMapping[node.id];

        foreach (var other in nodesToCheckAgainst)
        {
            var otherCorners = cellIdToCornerPointsMapping[other.id];
            if (myCorners.Overlaps(otherCorners))
            {
                throw new DiagramNodeException(node, $"Non-related nodes overlap: {node.DescribeShort()} with {other.DescribeShort()}.\nSee {HelpUrl} for how to fix.");
            }
        }

        if (mxCellMap[node.id].isCollapsed)
        {
            collisionRootsToCheck.Enqueue(node);
        }
        else
        {
            foreach (var child in node.children)
            {
                Process(child);
            }
        }

        nodesToCheckAgainst.Add(node);
    }
}
