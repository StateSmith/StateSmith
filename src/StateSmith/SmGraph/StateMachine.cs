using StateSmith.Common;
using StateSmith.SmGraph.Visitors;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace StateSmith.SmGraph;

/// <summary>
/// Allow state machines to be nested. Why? Allows you to test
/// that section of the state machine design independently of the rest.
/// Very helpful in large designs.
/// </summary>
public class StateMachine : NamedVertex
{
    /// <summary>
    /// Prefer using <see cref="GetEventListCopy"/> if possible. This field is populated near the end of the transformation pipeline. <see cref="AddUsedEventsToSmClass"/>.
    /// </summary>
    public HashSet<string> _events = new();
    public List<HistoryVertex> historyStates = new();
    public string variables = "";

    public StateMachine(string name) : base(name)
    {
    }

    public List<string> GetEventListCopy()
    {
        List<string> list = _events.ToList();
        list.Sort();
        return list;
    }

    public IReadOnlySet<string> GetEventSet()
    {
        return _events;
    }

    /// <summary>
    /// Includes self
    /// </summary>
    /// <returns></returns>
    public List<NamedVertex> GetNamedVerticesCopy()
    {
        SmToNamedVerticesVisitor visitor = new();
        this.Accept(visitor);
        return visitor.namedVertices;
    }

    public override void Accept(IVertexVisitor visitor)
    {
        visitor.Visit(this);
    }
}
