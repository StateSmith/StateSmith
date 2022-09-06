using StateSmith.Common;
using StateSmith.compiler.Visitors;

namespace StateSmith.Compiling
{
    public class DefaultToDoEventVisitor : NamedVisitor
    {
        public override void Visit(NamedVertex v)
        {
            foreach (var b in v.Behaviors)
            {
                if (b.HasAtLeastOneTrigger() == false)
                {
                    b.triggers.Add(TriggerHelper.TRIGGER_DO);
                }
            }

            VisitVertexChildren(v, this);
        }

        public override void Visit(Vertex v)
        {
            throw new System.NotImplementedException("should not happen");
        }

        public override void Visit(NotesVertex v)
        {
            //throw new System.NotImplementedException();
        }

        public override void Visit(InitialState v)
        {
            //throw new System.NotImplementedException();
        }
    }
}
