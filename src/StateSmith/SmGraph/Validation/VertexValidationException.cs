#nullable enable
using System;

namespace StateSmith.SmGraph.Validation
{
    public class VertexValidationException : Exception
    {
        public Vertex vertex;

        public VertexValidationException(Vertex vertex, string message) : base(message)
        {
            this.vertex = vertex;
        }
    }
}
