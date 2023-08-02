#nullable enable

using StateSmith.SmGraph.Visitors;

namespace StateSmith.SmGraph;

/// <summary>
/// Includes regular states, super states, orthogonal states and state machines
/// </summary>
public abstract class NamedVertex : Vertex
{
    internal string _name;
    public string Name => _name;

    /// <summary>
    /// todo_low - remove
    /// </summary>
    public bool nameIsGloballyUnique = false;

    public NamedVertex(string name)
    {
        this._name = name;
    }

    public void Rename(string newName)
    {
        _name = newName;
    }

    public override void Accept(IVertexVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return GetType().Name + ": " + Name;
    }
}
