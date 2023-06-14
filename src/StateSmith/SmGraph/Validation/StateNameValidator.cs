#nullable enable
using StateSmith.SmGraph.Visitors;
using System;
using System.Collections.Generic;

namespace StateSmith.SmGraph.Validation;

public class StateNameValidator : NamedVisitor
{
    readonly Dictionary<string, NamedVertex> stateNames = new(StringComparer.OrdinalIgnoreCase);

    public override void Visit(Vertex v)
    {
        VisitChildren(v);
    }

    public override void Visit(NamedVertex v)
    {
        ValidateStateName(v);
        ValidateUniqueStateName(v);

        Visit((Vertex)v);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/199
    /// </summary>
    private static void ValidateStateName(NamedVertex namedVertex)
    {
        const string rootStateName = "ROOT";
        if (string.Equals(namedVertex.Name, rootStateName, System.StringComparison.OrdinalIgnoreCase))
        {
            throw new VertexValidationException(namedVertex, $"`{rootStateName}` is a reserved state name. Please pick another.");
        }
    }

    private void ValidateUniqueStateName(Vertex v)
    {
        if (v is NamedVertex namedVertex)
        {
            var name = namedVertex.Name;
            if (stateNames.TryGetValue(name, out var otherState))
            {
                throw new VertexValidationException(v, $"Duplicate state name `{name}` also used by state `{VertexPathDescriber.Describe(otherState)}`.");
            }
            stateNames.Add(name, namedVertex);
        }
    }
}
