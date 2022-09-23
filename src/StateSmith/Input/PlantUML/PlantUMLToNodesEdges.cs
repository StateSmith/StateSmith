using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using StateSmith.Input.antlr4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace StateSmith.Input.PlantUML
{
    public class PlantUMLToNodesEdges
    {
        private PlantUMLWalker walker = new();

        public ErrorListener errorListener = new ErrorListener();

        public List<DiagramEdge> Edges => walker.edges;
        public DiagramNode Root => walker.root;

        public void ParseDiagram(string plantUMLDiagramText)
        {
            PlantUMLParser parser = BuildParserForString(plantUMLDiagramText);
            IParseTree tree = parser.diagram();
            ParseTreeWalker.Default.Walk(walker, tree);
        }

        public bool HasError()
        {
            return errorListener.errors.Count > 0;
        }

        public List<Error> GetErrors()
        {
            return errorListener.errors;
        }

        private PlantUMLParser BuildParserForString(string inputString)
        {
            ICharStream stream = CharStreams.fromString(inputString);
            ITokenSource lexer = new PlantUMLLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            PlantUMLParser parser = new(tokens)
            {
                BuildParseTree = true
            };
            parser.AddErrorListener(errorListener);

            return parser;
        }
    }


}
