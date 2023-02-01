using StateSmith.Compiling;
using System.Collections.Generic;

namespace StateSmith.compiler.Visitors
{
    public abstract class VertexVisitor
    {
        public abstract void Visit(Vertex v);
        public abstract void Visit(NamedVertex v);
        public abstract void Visit(State v);
        public abstract void Visit(OrthoState v);
        public abstract void Visit(StateMachine v);
        public abstract void Visit(NotesVertex v);
        public abstract void Visit(InitialState v);

        public virtual void Visit(ChoicePoint v) => Visit((Vertex)v);
        public virtual void Visit(EntryPoint v) => Visit((Vertex)v);
        public virtual void Visit(ExitPoint v) => Visit((Vertex)v);
        public virtual void Visit(HistoryVertex v) => Visit((Vertex)v);
        public virtual void Visit(HistoryContinueVertex v) => Visit((Vertex)v);

        public static void VisitVertexChildren(Vertex v, VertexVisitor visitor)
        {
            //copy list so that we can remove children while iterating.
            //TODO Not ideal. What if larger scale changes made? requires thought.
            var childrenCopy = new List<Vertex>(v.Children);
            foreach (var child in childrenCopy)
            {
                child.Accept(visitor);
            }
        }

        public virtual void VisitChildren(Vertex v)
        {
            VisitVertexChildren(v, this);
        }
    }
}
