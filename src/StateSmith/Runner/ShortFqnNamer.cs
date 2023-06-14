#nullable enable

using StateSmith.Common;
using StateSmith.SmGraph;
using StateSmith.SmGraph.Validation;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.Runner;

/// <summary>
/// Resolves state name conflicts using a short Fully Qualified Name.
/// https://github.com/StateSmith/StateSmith/issues/138
/// </summary>
public class ShortFqnNamer
{
    /// <summary>
    /// Specifies how to resolve naming conflict.
    /// https://github.com/StateSmith/StateSmith/issues/138#issuecomment-1470041906
    /// </summary>
    private readonly bool resolveWithHighestState;

    public ShortFqnNamer(bool resolveWithAncestor = true)
    {
        this.resolveWithHighestState = resolveWithAncestor;
    }

    public void ResolveNameConflicts(StateMachine sm)
    {
        NamedVertexMap nameMap = new(sm, skipRoot: true);

        sm.VisitTypeRecursively<NamedVertex>(skipSelf: true, (vertex =>
        {
            List<NamedVertex> conflictingVertices = nameMap.GetNamedVerticesWithName(vertex.Name);
            if (conflictingVertices.Count > 1)
            {
                ResolveNameConflict(vertex, conflictingVertices);
            }
        }));
    }

    private void ResolveNameConflict(NamedVertex vertex, List<NamedVertex> verticesWithSameName)
    {
        NamedVertex? ancestorForPrefix;

        if (resolveWithHighestState == false)
        {
            ancestorForPrefix = (NamedVertex)vertex.NonNullParent; // parent name must be unique at this point
        }
        else
        {
            // This might be a bit confusing at first. We are using Least Common Ancestor path info to
            // help resolve name conflicts. See https://github.com/StateSmith/StateSmith/issues/138#issuecomment-1469374108
            TransitionPath shortestPath = FindShortestPath(vertex, verticesWithSameName);

            if (IsTransitionToDirectParent(shortestPath))
            {
                // Use parent for name.
                ancestorForPrefix = vertex.ParentState.ThrowIfNull();
            }
            else if (IsTransitionToDescendant(shortestPath))
            {
                // All name collisions are with descendants. Leave name as is.
                ancestorForPrefix = null;
            }
            else
            {
                ancestorForPrefix = (NamedVertex)shortestPath.toExit.Last();
            }
        }

        if (ancestorForPrefix != null)
            vertex.Rename(ancestorForPrefix.Name + "__" + vertex.Name);
    }

    private static TransitionPath FindShortestPath(NamedVertex vertex, List<NamedVertex> verticesWithSameName)
    {
        TransitionPath? shortestPath = null;

        foreach (var otherVertex in verticesWithSameName)
        {
            if (vertex == otherVertex) continue;

            var path = vertex.FindTransitionPathTo(otherVertex);

            if (IsPathToSibling(path))
            {
                throw new VertexValidationException(vertex.NonNullParent, $"State has multiple children with the same name `{vertex.Name}`");
            }

            if (shortestPath == null || IsNewShortestPath(shortestPath, path))
            {
                shortestPath = path;
            }
        }

        return shortestPath.ThrowIfNull();
    }

    private static bool IsTransitionToDirectParent(TransitionPath shortestPath)
    {
        return shortestPath!.toExit.Count == 1;
    }

    private static bool IsPathToSibling(TransitionPath path)
    {
        return path.toExit.Count == 1 && path.toEnter.Count == 1;
    }

    private static bool IsNewShortestPath(TransitionPath shortestPath, TransitionPath path)
    {
        // ignore paths to descendants
        if (IsTransitionToDescendant(path))
            return false;

        return path.toExit.Count < shortestPath.toExit.Count;
    }

    private static bool IsTransitionToDescendant(TransitionPath path)
    {
        return path.toExit.Count == 0;  // this will happen if compared against descendant state with same name
    }
}
