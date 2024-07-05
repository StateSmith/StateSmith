#nullable enable

using System.Collections.Generic;
using System.Text;
using StateSmith.Output;

namespace StateSmith.Input;

/// <summary>
/// Diagram agnostic (draw.io, plantuml, yed) node type.
/// Could end up being a State, a directive, a comment, ...
/// </summary>
public class DiagramNode
{
    public string id = "";

    /// <summary>
    /// Used to track the diagram IDs of other diagram elements that make up this DiagramNode.
    /// One example is a state's nested handler node like: https://github.com/StateSmith/StateSmith/wiki/Getting-started-using-draw.io-with-StateSmith#restrictions-on-styling-
    /// </summary>
    public List<string> subIds = new();
    public string label = "";
    public DiagramNode? parent;
    public List<DiagramNode> children = new();

    public override string ToString()
    {
        return $"DiagramNode{{id:{id}}}";
    }

    /// <summary>
    /// internal for now. Functionality may be moved.
    /// </summary>
    internal string DescribeShort()
    {
        string shortLabel = label;
        shortLabel = StringUtils.MaybeTruncateWithEllipsis(shortLabel, 15);
        shortLabel = StringUtils.ReplaceNewLineChars(shortLabel, "\\n");
        shortLabel = StringUtils.EscapeCharsForString(shortLabel);
        return $"{{id:\"{id.Trim()}\", label:\"{shortLabel}\"}}";
    }

    /// <summary>
    /// internal for now. Functionality may be moved.
    /// </summary>
    /// <param name="stringBuilder"></param>
    internal void Describe(StringBuilder stringBuilder)
    {
        const string tab = "    ";
        string indentedLabel = StringUtils.ReplaceNewLineChars(label.Trim(), $"\n{tab}        ");
        string message =
                $"{nameof(DiagramNode)}:\n" +
                $"{tab}id: {id}\n" +
                $"{tab}label: `{indentedLabel}`\n" +
                $"{tab}parent.id: {parent?.id ?? "null"}" +
                "";
        stringBuilder.Append(message);
    }

    /// <summary>
    /// internal for now. Functionality may be moved.
    /// </summary>
    internal static void FullyDescribe(DiagramNode? node, StringBuilder stringBuilder)
    {
        if (node == null)
        {
            stringBuilder.Append("null ").Append(nameof(DiagramNode)).Append('\n');
            return;
        }

        var currentNode = node;

        while (currentNode != null)
        {
            currentNode.Describe(stringBuilder);
            currentNode = currentNode.parent;

            if (currentNode != null)
            {
                stringBuilder.Append(" (parent follows below)\n");
            }

            stringBuilder.Append('\n');
        }
    }

    /// <summary>
    /// internal for now. Functionality may be moved.
    /// </summary>
    internal static string FullyDescribe(DiagramNode? node)
    {
        var sb = new StringBuilder();
        FullyDescribe(node, sb);
        return sb.ToString();
    }
}
