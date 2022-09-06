using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using StateSmith.Input.Expansions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StateSmith.Input.antlr4
{
    public class Error
    {
        public IToken offendingSymbol;
        public int line;
        public int column;
        public string message;
        public RecognitionException exception;

        public string BuildMessage()
        {
            return $"{message} at line {line} column {column}. Offending symbol: `{offendingSymbol.Text}`";
        }
    }

    public class ErrorListener : IAntlrErrorListener<IToken>
    {
        public List<Error> errors = new List<Error>();

        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            errors.Add(new Error() {
                offendingSymbol = offendingSymbol,
                line = line,
                column = charPositionInLine,
                message = msg,
                exception = e
            });
        }
    }

    public class LabelParser
    {
        public ErrorListener errorListener = new ErrorListener();

        public Node ParseNodeLabel(string stateLabel)
        {
            Grammar1Parser parser = BuildParserForString(stateLabel);

            IParseTree tree = parser.node();
            NodeEdgeWalker walker = WalkTree(tree);
            walker.node.tree = tree;

            if (walker.node is StateNode stateNode)
            {
                stateNode.behaviors = walker.behaviors;
            }

            return walker.node;
        }

        public bool HasError()
        {
            return errorListener.errors.Count > 0;
        }

        public List<Error> GetErrors()
        {
            return errorListener.errors;
        }

        public List<NodeBehavior> ParseEdgeLabel(string edgeLabel)
        {
            Grammar1Parser parser = BuildParserForString(edgeLabel);

            IParseTree tree = parser.edge();
            NodeEdgeWalker walker = WalkTree(tree);
            return walker.behaviors;
        }

        public string ParseAndVisitAnyCode(Grammar1BaseVisitor<string> visitor, string code)
        {
            Grammar1Parser parser = BuildParserForString(code);
            IParseTree tree = parser.any_code();
            return visitor.Visit(tree);
        }

        private NodeEdgeWalker WalkTree(IParseTree tree)
        {
            NodeEdgeWalker walker = new NodeEdgeWalker();
            ParseTreeWalker.Default.Walk(walker, tree);
            return walker;
        }

        private Grammar1Parser BuildParserForString(string inputString)
        {
            ICharStream stream = CharStreams.fromString(inputString);
            ITokenSource lexer = new Grammar1Lexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            Grammar1Parser parser = new Grammar1Parser(tokens)
            {
                BuildParseTree = true
            };
            parser.AddErrorListener(errorListener);

            return parser;
        }
    }
}
