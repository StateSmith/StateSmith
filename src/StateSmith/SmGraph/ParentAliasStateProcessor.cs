using StateSmith.SmGraph.Validation;
using StateSmith.SmGraph.Visitors;
using System.Diagnostics;
using System.Linq;

namespace StateSmith.SmGraph;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/2
/// Still needs thorough testing.
/// </summary>
public class ParentAliasStateProcessor : RecursiveVertexVisitor
{
    // https://github.com/StateSmith/StateSmith/issues/2
    public static void Process(Vertex vertex)
    {
        var processor = new ParentAliasStateProcessor();
        vertex.Accept(processor);
    }

    public override void Visit(State alias)
    {
        if (alias.Name.ToLower() != "$parent_alias")
        {
            VisitChildren(alias);
            return;
        }

        // this is a parent alias
        if (alias.Children.Count > 0)
        {
            throw new VertexValidationException(alias, "parent alias nodes cannot have children");
        }

        var parent = alias.Parent;

        if (parent == null)
        {
            throw new VertexValidationException(alias, "parent alias cannot be root node");
        }

        foreach (var incomingTransition in alias.IncomingTransitions.ToList())
        {
            incomingTransition.RetargetTo(parent);
        }

        foreach (var b in alias.Behaviors)
        {
            parent.AddBehavior(b);
        }

        alias._behaviors.Clear();
        parent.RemoveChild(alias);
    }
}
