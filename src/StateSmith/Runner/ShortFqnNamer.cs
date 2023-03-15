using StateSmith.Common;
using StateSmith.SmGraph;
using StateSmith.SmGraph.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.Runner;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/138
/// </summary>
public class ShortFqnNamer
{
    HashSet<Tuple<State, string>> childNameClaims = new();

    public void Process(StateMachine sm)
    {
        BreadthLayerWalker layerWalker = new();

        layerWalker.visitAction = (layerVertices) =>
        {
            // find any name collisions
            HashList<string, State> nameHashList = new();

            foreach (var vertex in layerVertices.OfType<State>()) {
                nameHashList.AddIfMissing(vertex.Name, vertex);
            }

            foreach (var key in nameHashList.GetKeys())
            {
                var statesWithKeyName = nameHashList.GetValues(key);

                if (statesWithKeyName.Count > 1)
                {
                    ResolveNameConflict(statesWithKeyName);
                }
            }
        };

        layerWalker.Walk(sm);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="statesWithKeyName">All at the same depth</param>
    /// <exception cref="NotImplementedException"></exception>
    private void ResolveNameConflict(List<State> statesWithKeyName)
    {
        // find common ancestor


    }
}

// FIXME also add validation step. No state can have two children with same name (ignore case).
