#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace StateSmith.SmGraph;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/359
/// </summary>
public class FlatSmRootSupport
{
    public static List<Vertex> SupportFlatSmRoot(List<Vertex> rootVertices)
    {
        // find a SINGLE root state machine node with no children

        var smRoots = rootVertices.OfType<StateMachine>();

        if (smRoots.Count() != 1)
            return rootVertices;

        var root = smRoots.First();

        if (root.Children.Count != 0)
        {
            return rootVertices;
        }

        // this is a flat state machine root!
        // take all other roots and make them children of this root
        foreach (var vertex in rootVertices)
        {
            if (vertex != root)
            {
                root.AddChild(vertex);
            }
        }

        return new List<Vertex> { root };
    }
}
