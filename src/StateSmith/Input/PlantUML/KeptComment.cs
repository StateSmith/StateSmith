#nullable enable

namespace StateSmith.Input.PlantUML;

/// <summary>
/// Block comments that start with `/'!` and end with `'/` are not ignored by PlantUML grammar.
/// They are required for features like https://github.com/StateSmith/StateSmith/issues/334
/// https://github.com/StateSmith/StateSmith/issues/335
/// </summary>
public class KeptComment
{
    public string diagramId;
    public string text;

    public KeptComment(string diagramId, string comment)
    {
        this.diagramId = diagramId;
        this.text = comment;
    }
}
