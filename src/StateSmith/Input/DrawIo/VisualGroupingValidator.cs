#nullable enable

using StateSmith.Compiling;
using System;
using System.Collections.Generic;

namespace StateSmith.Input.DrawIo;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/81
/// </summary>
public class VisualGroupingValidator
{
    Dictionary<string, CornerPoints> cellIdToCornerPointsMapping = new();
    Dictionary<string, MxCell> mxCellMap = new();

    public void Process(Dictionary<string, MxCell> mxCellMap, List<DiagramNode> roots)
    {
        this.mxCellMap = mxCellMap;

        foreach (var node in roots)
        {
            AddCornerPointsRecursively(node);
        }

        // traverse from parent to child. ensure all children are in parent
        foreach (var node in roots)
        {
            EnsureChildrenContainedRecursively(node);
        }

        var notTouchingValidator = new NotTouchingValidator(cellIdToCornerPointsMapping, mxCellMap);
        notTouchingValidator.ProcessTopLevelNodes(roots);
    }

    private void EnsureChildrenContainedRecursively(DiagramNode node)
    {
        if (mxCellMap[node.id].isCollapsed)
        {
            // If the group is collapsed, children can be anywhere and we don't care.
            // They are guaranteed to be contained in the diagram.
        }
        else
        {
            EnsureChildrenContained(node);
        }

        foreach (var child in node.children)
        {
            EnsureChildrenContainedRecursively(child);
        }
    }

    private void EnsureChildrenContained(DiagramNode node)
    {
        var myPoints = cellIdToCornerPointsMapping[node.id];
        foreach (var child in node.children)
        {
            var childPoints = cellIdToCornerPointsMapping[child.id];

            const string errMessagePrefix = "Child outside of parent grouping";

            if (childPoints.left < myPoints.left)
            {
                throw new DiagramNodeException(child, $"{errMessagePrefix} (to the left).");
            }

            if (childPoints.top < myPoints.top)
            {
                throw new DiagramNodeException(child, $"{errMessagePrefix} (above).");
            }

            if (childPoints.right > myPoints.right)
            {
                throw new DiagramNodeException(child, $"{errMessagePrefix} (to the right).");
            }

            if (childPoints.bottom > myPoints.bottom)
            {
                throw new DiagramNodeException(child, $"{errMessagePrefix} (below).");
            }
        }
    }

    private void AddCornerPointsRecursively(DiagramNode node)
    {
        CornerPoints parentPoints;

        if (node.parent == null || IsCollapsed(node.parent))
        {
            parentPoints = new();
        }
        else
        {
            parentPoints = cellIdToCornerPointsMapping[node.parent.id];
        }

        var cell = mxCellMap[node.id];

        CornerPoints points = new();
        MxBounds bounds = GetPrimaryBounds(cell);
        points.left = parentPoints.left + bounds.x;
        points.top = parentPoints.top + bounds.y;
        points.right = points.left + bounds.width;
        points.bottom = points.top + bounds.height;

        cellIdToCornerPointsMapping[node.id] = points;

        foreach (var child in node.children)
        {
            AddCornerPointsRecursively(child);
        }
    }

    private static MxBounds GetPrimaryBounds(MxCell cell)
    {
        return cell.primaryBounds ?? new();
    }

    private bool IsCollapsed(DiagramNode parent_node)
    {
        return mxCellMap[parent_node.id].isCollapsed;
    }
}
