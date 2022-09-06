using System;

namespace StateSmith.Compiling
{
    public class VertexValidationException : Exception
    {
        public VertexValidationException(Vertex vertex, string message) : base(message)
        {
        }
    }
}
