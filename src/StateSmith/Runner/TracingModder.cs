#nullable enable

using StateSmith.Common;
using StateSmith.SmGraph.Visitors;
using StateSmith.SmGraph;
using StateSmith.Output;
using System.Linq;

namespace StateSmith.Runner;

public class TracingModder : NamedVisitor
{
    protected string semiColon;

    public TracingModder(string semiColon = ";")
    {
        this.semiColon = semiColon;
    }

    public void AddTracingBehaviors(StateMachine stateMachine)
    {
        Visit(stateMachine);
    }

    public override void Visit(Vertex v)
    {
        var transitions = v.Behaviors.Where(b => b.HasTransition());

        foreach (var t in transitions)
        {
            var tracingCode = $"trace(\"Transition action `{StringUtils.EscapeCharsForString(t.actionCode)}` for {Vertex.Describe(v)} to {Vertex.Describe(t.TransitionTarget)}.\"){semiColon}";

            if (t.HasActionCode())
            {
                t.actionCode = tracingCode + "\n" + t.actionCode;
            }
            else
            {
                t.actionCode = tracingCode;
            }
        }

        VisitChildren(v);
    }

    public override void Visit(NamedVertex v)
    {
        foreach (var b in v.Behaviors)
        {
            var uml = b.DescribeAsUml();
            var originalGuard = b.guardCode ?? "";

            if (originalGuard.Length == 0)
            {
                originalGuard = "true";
            }

            string escapedUml = StringUtils.EscapeCharsForString(uml);
            var start = $"trace_guard(\"State {v.Name}: check behavior `{escapedUml}`.\", ";
            var end = ")";
            
            var newGuard = $"{start}{originalGuard}{end}";
            b.guardCode = newGuard;
        }

        Visit((Vertex)v);
        v.AddBehavior(index: 0, behavior: new Behavior(trigger: TriggerHelper.TRIGGER_ENTER, guardCode: "", actionCode: $"trace(\"Enter {v.Name}.\"){semiColon}"));
        v.AddBehavior(index: 0, behavior: new Behavior(trigger: TriggerHelper.TRIGGER_EXIT, guardCode: "", actionCode: $"trace(\"Exit {v.Name}.\"){semiColon}"));
    }

    public override void Visit(NotesVertex v)
    {
        // ignore
    }

    public override void Visit(InitialState v)
    {
        Visit((Vertex)v);
    }
}
