using StateSmith.Compiling;
using System.Collections.Generic;
using System.Text;

namespace StateSmith.compiler.Visitors
{
    public class VertexPathDescriber
    {
        public static string Describe(Vertex vertex)
        {
            if (vertex == null)
            {
                return "";
            }

            Stack<Vertex> reversedVertices = GetReversedPathVertices(vertex);

            StringBuilder stringBuilder = new StringBuilder();
            ShortDescribingVisitor visitor = new ShortDescribingVisitor(stringBuilder);

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

        private static Stack<Vertex> GetReversedPathVertices(Vertex vertex)
        {
            Stack<Vertex> reversedVertices = new Stack<Vertex>();

            while (vertex != null)
            {
                reversedVertices.Push(vertex);
                vertex = vertex.Parent;
            }

            return reversedVertices;
        }
    }
}
