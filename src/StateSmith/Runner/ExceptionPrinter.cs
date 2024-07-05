#nullable enable

using StateSmith.SmGraph.Visitors;
using StateSmith.SmGraph;
using StateSmith.Input;
using System.Text;
using System.IO;
using StateSmith.SmGraph.Validation;

namespace StateSmith.Runner;

public class ExceptionPrinter
{
    readonly IConsolePrinter consolePrinter;
    const string SsGrammarRelatedHelpMsg = ">>>> RELATED HELP <<<<\nhttps://github.com/StateSmith/StateSmith/issues/174";

    public ExceptionPrinter(IConsolePrinter consolePrinter)
    {
        this.consolePrinter = consolePrinter;
    }

    public void DumpExceptionDetails(System.Exception exception, string filePath)
    {
        StringBuilder sb = new();
        BuildExceptionDetails(sb, exception, additionalDetail: true);
        File.WriteAllText(filePath, sb.ToString());
    }

    public void PrintException(System.Exception exception)
    {
        StringBuilder sb = new();
        BuildExceptionDetails(sb, exception);
        consolePrinter.WriteErrorLine(sb.ToString());
    }

    public void BuildExceptionDetails(StringBuilder sb, System.Exception exception, bool additionalDetail = false, int depth = 0)
    {
        if (depth > 0)
        {
            sb.AppendLine("==========================");
            sb.AppendLine("Caused by below exception:");
        }

        string? customMessage = TryBuildingCustomExceptionDetails(exception);
        if (customMessage != null)
        {
            sb.AppendLine(customMessage);
        }

        if (additionalDetail || customMessage == null)
        {
            sb.Append($"Exception {exception.GetType().Name} : ");
            sb.AppendLine($"{exception.Message}");
        }

        if (additionalDetail)
        {
            sb.AppendLine($"{exception.StackTrace}");
        }

        if (exception.InnerException != null)
        {
            BuildExceptionDetails(sb, exception.InnerException, additionalDetail, depth + 1);
        }
    }

    public string OutputEdgeDetails(DiagramEdge edge)
    {
        StringBuilder sb = new();
        edge.Describe(sb);
        sb.Append("==========================\n");
        sb.Append("EDGE SOURCE NODE:\n");
        edge.source.Describe(sb);
        sb.Append("\n==========================\n");
        sb.Append("EDGE TARGET NODE:\n");
        edge.target.Describe(sb);

        return sb.ToString();
    }

    public static string BuildReasonsString(string reasons)
    {
        string str = "Reason(s): " + reasons.ReplaceLineEndings("\n           ");
        return str;
    }

    public string? TryBuildingCustomExceptionDetails(System.Exception ex)
    {
        switch (ex)
        {
            case DiagramEdgeParseException parseException:
                {
                    DiagramEdge edge = parseException.edge;
                    string reasons = ex.Message;
                    reasons = BuildReasonsString(reasons);
                    string fromString = VertexPathDescriber.Describe(parseException.sourceVertex);
                    string toString = VertexPathDescriber.Describe(parseException.targetVertex);
                    string message = $@"Failed parsing diagram edge
from: {fromString}
to:   {toString}
Edge label: `{edge.label}`
{reasons}
Edge diagram id: {edge.id}

{SsGrammarRelatedHelpMsg}
";
                    return message;
                }

            case DiagramEdgeException diagramEdgeException:
                {
                    string message = diagramEdgeException.Message;
                    message += OutputEdgeDetails(diagramEdgeException.edge);
                    return message;
                }

            case DiagramNodeException diagramNodeException:
                {
                    string message = diagramNodeException.Message;
                    message += "\n" + DiagramNode.FullyDescribe(diagramNodeException.Node);

                    if (diagramNodeException is DiagramNodeParseException)
                    {
                        message += $"\n{SsGrammarRelatedHelpMsg}";
                    }

                    return message;
                }

            case VertexValidationException vertexValidationException:
            {
                string message = nameof(VertexValidationException) + ": " + vertexValidationException.Message;

                var vertex = vertexValidationException.vertex;
                string fromString = VertexPathDescriber.Describe(vertex);

message += $@"
Vertex:
    Path: {fromString}
    Diagram Id: {vertex.DiagramId}
    Children count: {vertex.Children.Count}
    Behaviors count: {vertex.Behaviors.Count}
    Incoming transitions count: {vertex.IncomingTransitions.Count}
";
                return message;
            }


            case BehaviorValidationException behaviorValidationException:
                {
                    string message = nameof(BehaviorValidationException) + ": " + behaviorValidationException.Message;

                    Behavior behavior = behaviorValidationException.behavior;
                    string fromString = VertexPathDescriber.Describe(behavior.OwningVertex);
                    string toString = VertexPathDescriber.Describe(behavior.TransitionTarget);

                    message += $@"
Behavior:
    Owning vertex: {fromString}
    Target vertex: {toString}
    Order: {behavior.GetOrderString()}
    Triggers: `{string.Join(", ", behavior.Triggers)}`
    Guard: `{behavior.guardCode}`
    Action: `{behavior.actionCode}`
    Via Entry: `{behavior.viaEntry}`
    Via Exit : `{behavior.viaExit}`
    Diagram Id: `{behavior.DiagramId}`
";

                    return message;
                }
        }

        return null;
    }
}
