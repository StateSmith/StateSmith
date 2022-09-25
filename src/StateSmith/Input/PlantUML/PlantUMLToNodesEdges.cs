﻿using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using StateSmith.Input.antlr4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace StateSmith.Input.PlantUML
{
    public class PlantUMLToNodesEdges
    {
        private const string PlantUmlEntryPointText = "entryPoint";
        private const string PlantUmlExitPointText = "exitPoint";

        private PlantUMLWalker walker = new();

        public ErrorListener errorListener = new ErrorListener();

        public List<DiagramEdge> Edges => walker.edges;
        public DiagramNode Root => walker.root;

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
        }

        private bool IsNodeEntryPoint(DiagramNode node)
        {
            if (walker.nodeStereoTypeLookup.TryGetValue(node, out var stereotype))
            {
                return stereotype == PlantUmlEntryPointText;
            }

            return false;
        }

        private bool IsNodeExitPoint(DiagramNode node)
        {
            if (walker.nodeStereoTypeLookup.TryGetValue(node, out var stereotype))
            {
                return stereotype == PlantUmlExitPointText;
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
                    edge.target = edge.target.parent;
                }

                // transitions from an exit point need to be adjusted
                if (IsNodeExitPoint(edge.source))
                {
                    edge.label += "via exit " + edge.source.label;
                    edge.source = edge.source.parent;
                }
            }

            foreach (var item in walker.nodeStereoTypeLookup)
            {
                var node = item.Key;
                var stereotype = item.Value;

                if (stereotype == PlantUmlEntryPointText)
                {
                    node.label = "entry : " + node.label;
                }
                else if (stereotype == PlantUmlExitPointText)
                {
                    node.label = "exit : " + node.label;
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
