using StateSmith.SmGraph;

namespace StateSmith.SmGraph.Visitors
{
    public abstract class NamedVisitor : VertexVisitor
    {
        public override void Visit(State v)
        {
            Visit((NamedVertex)v);
        }

        public override void Visit(OrthoState v)
        {
            Visit((NamedVertex)v);
        }

        public override void Visit(StateMachine v)
        {
            Visit((NamedVertex)v);
        }
    }
}
