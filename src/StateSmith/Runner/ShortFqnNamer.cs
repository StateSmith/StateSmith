#nullable enable

using StateSmith.Common;
using StateSmith.SmGraph;
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
        TransitionPath? shortestPath = null;

        foreach (var otherVertex in verticesWithSameName)
        {
            if (vertex == otherVertex) continue;

            var path = vertex.FindTransitionPathTo(otherVertex);

            if (path.toExit.Count == 1 && path.toEnter.Count == 1)
            {
                throw new VertexValidationException(vertex.NonNullParent, $"State has multiple children with the same name `{vertex.Name}`");
            }

            if (shortestPath == null || IsNewShortestPath(shortestPath, path))
            {
                shortestPath = path;
            }
        }

        NamedVertex namedAncestor;

        if (resolveWithHighestState == false)
        {
            namedAncestor = (NamedVertex)vertex.NonNullParent; // parent name must be unique at this point
        }
        else
        {
            if (shortestPath!.toExit.Count == 1)
            {
                namedAncestor = (NamedVertex)shortestPath.leastCommonAncestor.ThrowIfNull();
            }
            else
            {
                namedAncestor = (NamedVertex)shortestPath.toExit.Last();
            }
        }

        vertex.Rename(namedAncestor.Name + "__" + vertex.Name);
    }

    private static bool IsNewShortestPath(TransitionPath shortestPath, TransitionPath path)
    {
        if (path.toExit.Count == 0) // this will happen if compared against child state with same name
            return false;

        return path.toExit.Count < shortestPath.toExit.Count;
    }
}
