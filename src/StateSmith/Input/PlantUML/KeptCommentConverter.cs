#nullable enable

namespace StateSmith.Input.PlantUML;

public class KeptCommentConverter
{
    public static DiagramNode? Convert(KeptComment keptComment)
    {
        var text = keptComment.text.Trim();

        if (!text.StartsWith("$CONFIG"))
        {
            return null;
        }

        var node = new DiagramNode
        {
            id = keptComment.diagramId,
            label = keptComment.text.Trim()
        };
        
        return node;
    }
}
