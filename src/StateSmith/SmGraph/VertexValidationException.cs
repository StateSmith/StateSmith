using System;

namespace StateSmith.SmGraph
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
