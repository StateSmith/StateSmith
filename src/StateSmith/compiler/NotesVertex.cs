using StateSmith.compiler.Visitors;

namespace StateSmith.Compiling
{
    public class NotesVertex : Vertex
    {
        public string notes;

        public override void Accept(VertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
