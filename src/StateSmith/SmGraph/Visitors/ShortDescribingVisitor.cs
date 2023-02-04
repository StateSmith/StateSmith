using StateSmith.SmGraph;
using System;
using System.Text;

namespace StateSmith.SmGraph.Visitors
{
    public class ShortDescribingVisitor : NamedVisitor
    {
        public StringBuilder stringBuilder;

        public ShortDescribingVisitor()
        {
            stringBuilder = new StringBuilder();
        }

        public ShortDescribingVisitor(StringBuilder stringBuilder)
        {
            this.stringBuilder = stringBuilder;
        }

        public override void Visit(Vertex v)
        {
            throw new NotImplementedException();
        }

        public override void Visit(RenderConfigVertex v)
        {
            stringBuilder.Append($"{v.GetType().Name}");
        }

        public override void Visit(ConfigOptionVertex v)
        {
            stringBuilder.Append($"{v.GetType().Name}{{{v.name}}}");
        }

        public override void Visit(EntryPoint v)
        {
            stringBuilder.Append($"{v.GetType().Name}{{{v.label}}}");
        }

        public override void Visit(ExitPoint v)
        {
            stringBuilder.Append($"{v.GetType().Name}{{{v.label}}}");
        }

        public override void Visit(NamedVertex v)
        {
            stringBuilder.Append($"{v.GetType().Name}{{{v.Name}}}");
        }

        public override void Visit(NotesVertex v)
        {
            stringBuilder.Append("$Notes");
        }

        public override void Visit(InitialState v)
        {
            stringBuilder.Append("$InitialState");
        }

        public static void Describe(StringBuilder stringBuilder, Vertex vertex)
        {
            var visitor = new ShortDescribingVisitor(stringBuilder);
            vertex.Accept(visitor);
        }
    }
}
