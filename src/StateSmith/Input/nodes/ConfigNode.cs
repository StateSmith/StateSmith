#nullable enable
namespace StateSmith.Input.Antlr4;

public class ConfigNode : Node
{
    public string name;
    public string value;

    public ConfigNode(string name, string value)
    {
        this.name = name;
        this.value = value;
    }
}
