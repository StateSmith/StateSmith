using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using StateSmith.Runner;
using System;
using System.Collections.Generic;

namespace StateSmith.Input.Antlr4;

public class LabelParser
{
    public ErrorListener errorListener = new();

    public Node ParseNodeLabel(string stateLabel)
    {
        StateSmithLabelGrammarParser parser = BuildParserForString(stateLabel);

        IParseTree tree = parser.node();
        NodeEdgeWalker walker = WalkTree(tree);
        walker.node.tree = tree;

        if (walker.node is INodeWithBehaviors nodeWithBehaviors)
        {
            nodeWithBehaviors.Behaviors = walker.behaviors;
        }

        return walker.node;
    }

    public void ThrowIfError(string badInput)
    {
        if (HasError())
        {
            var message = $"Failed parsing: `{badInput}`.\n" + ExceptionPrinter.BuildReasonsString(BuildErrorMessage(separator: "\n"));
            throw new FormatException(message);
        }
    }

    public string BuildErrorMessage(string separator)
    {
        return AntlrError.ErrorsToReasonStrings(GetErrors(), separator: separator);
    }

    public bool HasError()
    {
        return errorListener.errors.Count > 0;
    }

    public List<AntlrError> GetErrors()
    {
        return errorListener.errors;
    }

    public List<NodeBehavior> ParseEdgeLabel(string edgeLabel)
    {
        StateSmithLabelGrammarParser parser = BuildParserForString(edgeLabel);
        IParseTree tree = parser.edge();
        NodeEdgeWalker walker = WalkTree(tree);
        return walker.behaviors;
    }

    public string ParseAndVisitAnyCode(StateSmithLabelGrammarBaseVisitor<string> visitor, string code)
    {
        StateSmithLabelGrammarParser parser = BuildParserForString(code);
        IParseTree tree = parser.any_code();
        return visitor.Visit(tree);
    }

    public void ParseAndWalkAnyCode(StateSmithLabelGrammarBaseListener listener, string code)
    {
        StateSmithLabelGrammarParser parser = BuildParserForString(code);
        IParseTree tree = parser.any_code();
        ParseTreeWalker.Default.Walk(listener, tree);
    }

    private NodeEdgeWalker WalkTree(IParseTree tree)
    {
        NodeEdgeWalker walker = new();
        ParseTreeWalker.Default.Walk(walker, tree);
        return walker;
    }

    private StateSmithLabelGrammarParser BuildParserForString(string inputString)
    {
        ICharStream stream = CharStreams.fromString(inputString);
        var lexer = new StateSmithLabelGrammarLexer(stream);
        lexer.RemoveErrorListeners(); // prevent antlr4 error output to console
        lexer.AddErrorListener(errorListener);

        ITokenStream tokens = new CommonTokenStream(lexer);
        StateSmithLabelGrammarParser parser = new(tokens)
        {
            BuildParseTree = true
        };
        parser.RemoveErrorListeners(); // prevent antlr4 error output to console
        parser.AddErrorListener(errorListener);

        return parser;
    }
}
