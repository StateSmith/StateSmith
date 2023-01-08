using System.Collections.Generic;

namespace StateSmith.Input.DrawIo;
#nullable enable

public class MxCell
{
    public enum Type { None, Edge, Vertex, VertexImage }

    public string id;
    public string? parent;
    public string? label;
    public Type type = Type.None;
    public string? source;
    public string? target;
    public string? style;

    public Dictionary<string, string> styleMap = new();

    public MxCell(string id)
    {
        this.id = id;
    }

    public bool HasMatchingStyle(string key, string value)
    {
        if (styleMap.TryGetValue(key, out var readValue))
        {
            return value == readValue;
        }

        return false;
    }
}
