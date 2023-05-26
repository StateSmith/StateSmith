#nullable enable
using StateSmith.SmGraph.Visitors;
using System.Xml.Linq;

namespace StateSmith.SmGraph;

public class ConfigOptionVertex : Vertex
{
    public string name = "";
    public string value = "";

    public ConfigOptionVertex(string name, string contents)
    {
        this.name = name;
        this.value = contents;
    }

    public override void Accept(IVertexVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return GetType().Name + ": " + name;
    }
}
