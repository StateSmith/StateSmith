using StateSmith.SmGraph.Visitors;

namespace StateSmith.SmGraph
{
    public class NotesVertex : Vertex
    {
        public string notes;

        public override void Accept(IVertexVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
