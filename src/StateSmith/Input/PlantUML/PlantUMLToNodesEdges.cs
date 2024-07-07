#nullable enable

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using StateSmith.Common;
using StateSmith.Input.Antlr4;
using System;
using System.Collections.Generic;
using System.IO;

namespace StateSmith.Input.PlantUML;

/// <summary>
/// This class is responsible for parsing a PlantUML diagram into diagram agnostic nodes and edges
/// that will be used to generate the state machine.
/// </summary>
public class PlantUMLToNodesEdges
{
    public List<DiagramEdge> Edges => walker.edges;
    public DiagramNode Root => walker.root;

    private readonly PlantUMLWalker walker = new();
    private readonly ErrorListener errorListener = new();

    public void ParseDiagramFile(string filepath)
    {
        var text = File.ReadAllText(filepath);
        ParseDiagramText(text);
    }

    public void ParseDiagramText(string plantUMLDiagramText)
    {
        PlantUMLParser parser = BuildParserForString(plantUMLDiagramText);
        IParseTree tree = parser.diagram();
        ParseTreeWalker.Default.Walk(walker, tree);
        PostProcessForEntryExit();
        PostProcessForChoicePoint();
        PostProcessForKeptComments();
    }

    private void PostProcessForKeptComments()
    {
        foreach (var keptComment in walker.keptCommentBlocks)
        {
            DiagramNode? diagramNode = KeptCommentConverter.Convert(keptComment);
            if (diagramNode != null)
            {
                walker.root.children.Add(diagramNode);
            }
        }
    }

    private bool IsNodeEntryPoint(DiagramNode node)
    {
        if (walker.nodeStereoTypeLookup.TryGetValue(node, out var stereotype))
        {
            return PlantUMLTextComparer.IsEntryPointStereotype(stereotype);
        }

        return false;
    }

    private bool IsNodeExitPoint(DiagramNode node)
    {
        if (walker.nodeStereoTypeLookup.TryGetValue(node, out var stereotype))
        {
            return PlantUMLTextComparer.IsExitPointStereotype(stereotype);
        }

        return false;
    }

    /// <summary>
    /// See https://github.com/StateSmith/StateSmith/issues/3
    /// </summary>
    private void PostProcessForEntryExit()
    {
        foreach (var edge in walker.edges)
        {
            // transitions coming into an entry point need to be adjusted
            if (IsNodeEntryPoint(edge.target))
            {
                edge.label += "via entry " + edge.target.label;
                edge.target = edge.target.parent.ThrowIfNull();
            }

            // transitions from an exit point need to be adjusted
            if (IsNodeExitPoint(edge.source))
            {
                edge.label += "via exit " + edge.source.label;
                edge.source = edge.source.parent.ThrowIfNull();
            }
        }

        foreach (var item in walker.nodeStereoTypeLookup)
        {
            var node = item.Key;
            var stereotype = item.Value;

            if (PlantUMLTextComparer.IsEntryPointStereotype(stereotype))
            {
                node.label = "entry : " + node.label;
            }
            else if (PlantUMLTextComparer.IsExitPointStereotype(stereotype))
            {
                node.label = "exit : " + node.label;
            }
        }
    }

    /// See https://github.com/StateSmith/StateSmith/issues/40
    private void PostProcessForChoicePoint()
    {
        foreach (var item in walker.nodeStereoTypeLookup)
        {
            var node = item.Key;
            var stereotype = item.Value;

            if (PlantUMLTextComparer.IsChoicePointStereotype(stereotype))
            {
                node.label = "$choice : " + node.label;
            }
        }
    }

    public DiagramNode GetDiagramNode(string id)
    {
        return walker.nodeMap[id];
    }

    public bool HasError()
    {
        return errorListener.errors.Count > 0;
    }

    public List<AntlrError> GetErrors()
    {
        return errorListener.errors;
    }

    private PlantUMLParser BuildParserForString(string inputString)
    {
        // Slight hack to work around ANTL4 limitation
        // https://github.com/StateSmith/StateSmith/issues/352
        const char START_OF_FILE = '\u0001';
        inputString = START_OF_FILE + inputString;

        ICharStream stream = CharStreams.fromString(inputString);
        var lexer = new PlantUMLLexer(stream);
        lexer.RemoveErrorListeners(); // prevent antlr4 error output to console
        lexer.AddErrorListener(errorListener);

        ITokenStream tokens = new CommonTokenStream(lexer);
        PlantUMLParser parser = new(tokens)
        {
            BuildParseTree = true
        };
        parser.RemoveErrorListeners(); // prevent antlr4 error output to console
        parser.AddErrorListener(errorListener);

        return parser;
    }
}
