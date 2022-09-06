using StateSmith.compiler.Visitors;
using System.Diagnostics;

namespace StateSmith.Compiling
{
    /// <summary>
    /// Includes regular states, super states, orthogonal states and state machines
    /// </summary>
    public class NamedVertex : Vertex
    {
        internal string _name;
        public string Name => _name;
        public bool nameIsGloballyUnique = false;

        public NamedVertex(string name)
        {
            this._name = name;
        }

        public override void Accept(VertexVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return GetType().Name + ": " + Name;
        }
    }
}
