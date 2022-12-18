using System.Collections.Generic;
using System.Linq;
using StateSmith.compiler.Visitors;

namespace StateSmith.Compiling
{
    public class SpecificVertexValidator : VertexVisitor
    {
        public override void Visit(InitialState initialState)
        {
            InitialStateValidator.Validate(initialState);
        }

        public override void Visit(ShallowHistoryVertex v)
        {
            //FIXME finish
        }

        public override void Visit(Vertex v)
        {
            throw new System.NotImplementedException();
        }

        public override void Visit(NamedVertex v)
        {
            throw new System.NotImplementedException();
        }

        public override void Visit(State v)
        {
            //FIXME finish
            VisitChildren(v);
        }

        public override void Visit(OrthoState v)
        {
            throw new System.NotImplementedException();
        }

        public override void Visit(Statemachine v)
        {
            if (v.Parent != null)
            {
                throw new VertexValidationException(v, "State machines cannot be nested, yet. See https://github.com/adamfk/StateSmith/issues/7");
            }

            // require initial state
            int initialStateCount = v.Children.OfType<InitialState>().Count();
            if (initialStateCount != 1)
            {
                throw new VertexValidationException(v, $"State machines must have exactly 1 initial state. Actual count: {initialStateCount}.");
            }

            VisitChildren(v);
        }

        public override void Visit(NotesVertex v)
        {
            if (v.IncomingTransitions.Count > 0)
            {
                throw new VertexValidationException(v, "Notes vertices cannot have any incoming transitions");
            }

            if (v.Behaviors.Count > 0)
            {
                throw new VertexValidationException(v, "Notes vertices cannot have any behaviors");
            }

            if (v.Children.Count > 0)
            {
                throw new VertexValidationException(v, "Notes vertices cannot have any children");
            }

            //note that transitions to state nodes within a notes node are caught when converting from Diagram nodes to Vertices
        }

        public override void Visit(ChoicePoint v)
        {
            ChoicePointValidator.Validate(v);
        }

        public override void Visit(EntryPoint entryPoint)
        {
            EntryPointValidator.Validate(entryPoint);
        }

        public override void Visit(ExitPoint v)
        {
            ExitPointValidator.Validate(v);
        }
    }
}
