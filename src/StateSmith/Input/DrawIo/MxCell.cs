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

    public MxCell(string id)
    {
        this.id = id;
    }
}
