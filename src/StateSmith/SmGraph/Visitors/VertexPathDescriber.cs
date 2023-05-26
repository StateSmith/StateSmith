#nullable enable

using System.Collections.Generic;
using System.Text;

namespace StateSmith.SmGraph.Visitors;

public class VertexPathDescriber
{
    public static string Describe(Vertex? vertex)
    {
        if (vertex == null)
        {
            return "";
        }

        Stack<Vertex> reversedVertices = GetReversedPathVertices(vertex);

        StringBuilder stringBuilder = new();
        ShortDescribingVisitor visitor = new(stringBuilder, skipParentForRelative:true);

        string appender = "";

        while (reversedVertices.Count > 0)
        {
            stringBuilder.Append(appender);
            appender = ".";
            vertex = reversedVertices.Pop();
            vertex.Accept(visitor);
        }

        return stringBuilder.ToString();
    }

    private static Stack<Vertex> GetReversedPathVertices(Vertex? vertex)
    {
        Stack<Vertex> reversedVertices = new();

        while (vertex != null)
        {
            reversedVertices.Push(vertex);
            vertex = vertex.Parent;
        }

        return reversedVertices;
    }
}
