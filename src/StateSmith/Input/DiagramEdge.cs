#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using StateSmith.Output;

namespace StateSmith.Input;

/// <summary>
/// Diagram agnostic (draw.io, plantuml, yed) edge/arrow between nodes.
/// </summary>
public class DiagramEdge
{
    public string id;
    public DiagramNode source;
    public DiagramNode target;
    public string label;

    public DiagramEdge(string id, DiagramNode source, DiagramNode target, string label = "")
    {
        this.id = id;
        this.source = source;
        this.target = target;
        this.label = label;
    }

    /// <summary>
    /// Don't parse or rely on this output. It may change.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{nameof(DiagramEdge)}{{id: {id}}}";
    }

    /// <summary>
    /// internal for now. Functionality may be moved.
    /// </summary>
    /// <param name="stringBuilder"></param>
    internal void Describe(StringBuilder stringBuilder)
    {
        string indentedLabel = StringUtils.ReplaceNewLineChars(label.Trim(), "\n\t        ");

        string message =
                $"{nameof(DiagramEdge)}\n" +
                $"\tid: {id}\n" +
                $"\tlabel: `{indentedLabel}`\n" +
                $"\tsource.id: {source?.id ?? "null"}\n" +
                $"\ttarget.id: {target?.id ?? "null"}\n" +
                "";
        stringBuilder.Append(message);
    }

    #region equals and hash
    public override bool Equals(object? obj)
    {
        return obj is DiagramEdge edge &&
               id == edge.id &&
               EqualityComparer<DiagramNode>.Default.Equals(source, edge.source) &&
               EqualityComparer<DiagramNode>.Default.Equals(target, edge.target) &&
               label == edge.label;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(id, source, target, label);
    }

    #endregion equals and hash
}
