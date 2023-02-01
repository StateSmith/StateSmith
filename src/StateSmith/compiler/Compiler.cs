using StateSmith.Input.antlr4;
using StateSmith.Input.Expansions;
using System;
using System.Collections.Generic;
using StateSmith.compiler;
using StateSmith.Input;
using StateSmith.compiler.Visitors;
using StateSmith.Runner;

#nullable enable

namespace StateSmith.Compiling
{
    public class LibVars
    {
        public string vars = "";
    }

    public class Compiler
    {
        public const string InitialStateString = "$initial_state";
        public const string HistoryStateString = "$h";
        public const string HistoryContinueString = "$hc";

        public List<Vertex> rootVertices = new();
        private List<string> eventNames = new();
        private Dictionary<DiagramNode, Vertex> diagramVertexMap = new();

        /// <summary>
        /// Call this method when you want to support a custom input source.
        /// </summary>
        /// <param name="rootNodes"></param>
        /// <param name="edges"></param>
        public void CompileDiagramNodesEdges(List<DiagramNode> rootNodes, List<DiagramEdge> edges)
        {
            foreach (var node in rootNodes)
            {
                var vertex = ProcessNode(node, parentVertex: null);

                if (vertex != null)
                    rootVertices.Add(vertex);
            }

            foreach (var edge in edges)
            {
                ProcessEdge(edge);
            }

            SetupRoots();
        }

        public void SetupRoots()
        {
            foreach (var v in rootVertices)
            {
                if (v is NamedVertex namedVertex)
                {
                    CompilerRunner.UpdateNamedDescendantsMapping(namedVertex);
                }
            }
        }

        private void ProcessEdge(Input.DiagramEdge edge)
        {
            try
            {
                ProcessEdgeInner(edge);
            }
            catch (Exception ex)
            {
                if (ex is DiagramEdgeParseException)
                {
                    throw;  // let it continue
                }

                DiagramEdgeException diagramEdgeException = new DiagramEdgeException(edge, $"Failed while converting {nameof(DiagramEdge)} with id: `{edge.id}` to state transition", ex);
                throw diagramEdgeException;
            }
        }

        private Vertex GetVertexFromNode(DiagramNode node)
        {
            if (NodeHasState(node))
            {
                return diagramVertexMap[node];
            }

            throw new DiagramNodeException(node, $"Could not find State for {nameof(DiagramNode)} with id `{node.id}`.");
        }

        private Vertex? TryGetVertexFromNode(DiagramNode node)
        {
            if (NodeHasState(node))
            {
                return diagramVertexMap[node];
            }

            return null;
        }

        private bool NodeHasState(DiagramNode node)
        {
            return diagramVertexMap.ContainsKey(node);
        }


        private void ProcessEdgeInner(DiagramEdge edge)
        {
            var sourceVertex = GetVertexFromNode(edge.source);
            var targetVertex = GetVertexFromNode(edge.target);

            LabelParser labelParser = new LabelParser();
            List<NodeBehavior> nodeBehaviors = labelParser.ParseEdgeLabel(edge.label);

            PrintAndThrowIfEdgeParseFail(edge, sourceVertex, targetVertex, labelParser);

            if (nodeBehaviors.Count == 0)
            {
                sourceVertex.AddBehavior(new Behavior(owningVertex: sourceVertex, transitionTarget: targetVertex, diagramId: edge.id));
            }

            foreach (var nodeBehavior in nodeBehaviors)
            {
                var behavior = ConvertBehavior(owningVertex: sourceVertex, targetVertex: targetVertex, nodeBehavior: nodeBehavior);
                behavior.DiagramId = edge.id;
                sourceVertex.AddBehavior(behavior);
            }
        }

        private static void PrintAndThrowIfEdgeParseFail(Input.DiagramEdge edge, Vertex sourceVertex, Vertex targetVertex, LabelParser labelParser)
        {
            if (labelParser.HasError())
            {
                throw new DiagramEdgeParseException(edge, sourceVertex, targetVertex, ParserErrorsToReasonStrings(labelParser.GetErrors(), "\n"));
            }
        }

        public static void ExpandBehavior(Expander expander, Behavior behavior)
        {
            if (behavior.actionCode != null)
            {
                behavior.actionCode = ExpandingVisitor.ParseAndExpandCode(expander, behavior.actionCode);
            }

            if (behavior.guardCode != null)
            {
                behavior.guardCode = ExpandingVisitor.ParseAndExpandCode(expander, behavior.guardCode);
            }
        }

        public void ExpandAllBehaviors(Expander expander)
        {
            foreach (var root in rootVertices)
            {
                root.VisitRecursively(vertex => {
                    foreach (var behavior in vertex.Behaviors)
                    {
                        ExpandBehavior(expander, behavior);
                    }
                });
            }
        }

        private Vertex? ProcessNode(Input.DiagramNode diagramNode, Vertex? parentVertex)
        {
            if (diagramNode.label == null || diagramNode.label.Trim() == "")
            {
                return null;
            }

            LabelParser labelParser = new();
            Node node = labelParser.ParseNodeLabel(diagramNode.label);
            PrintAndThrowIfNodeParseFail(diagramNode, parentVertex, labelParser);

            Vertex thisVertex;
            bool visitChildren = true;

            switch (node)
            {
                default:
                    throw new ArgumentException("Unknown node: " + node);

                case NotesNode notesNode:
                    {
                        var noteVertex = new NotesVertex();
                        noteVertex.notes = notesNode.notes;
                        thisVertex = noteVertex;
                        visitChildren = true;
                        break;
                    }

                case StateMachineNode stateMachineNode:
                    {
                        var sm = new Statemachine(stateMachineNode.name);
                        sm.nameIsGloballyUnique = true;
                        thisVertex = sm;
                        break;
                    }

                case StateNode stateNode:
                    {
                        if (stateNode is OrthoStateNode orthoStateNode)
                        {
                            var orthoState = new OrthoState(stateNode.stateName);
                            thisVertex = orthoState;
                            orthoState.order = Double.Parse(orthoStateNode.order);
                            SetStateFromStateNode(stateNode, orthoState);
                        }
                        else
                        {
                            if (string.Equals(stateNode.stateName, InitialStateString, StringComparison.OrdinalIgnoreCase))
                            {
                                thisVertex = new InitialState();
                            }
                            else if (string.Equals(stateNode.stateName, HistoryStateString, StringComparison.OrdinalIgnoreCase))
                            {
                                thisVertex = new HistoryVertex();
                            }
                            else if (string.Equals(stateNode.stateName, HistoryContinueString, StringComparison.OrdinalIgnoreCase))
                            {
                                thisVertex = new HistoryContinueVertex();
                            }
                            else
                            {
                                var state = new State(stateNode.stateName);
                                thisVertex = state;
                                SetStateFromStateNode(stateNode, state);
                            }
                        }

                        ConvertBehaviors(thisVertex, stateNode);
                        break;
                    }

                case EntryPointNode pointNode:
                    {
                        thisVertex = new EntryPoint(pointNode.label);
                        
                        if (diagramNode.children.Count > 0)
                        {
                            throw new DiagramNodeException(diagramNode, $"entry points cannot have children. See https://github.com/StateSmith/StateSmith/issues/3");
                        }
                        break;
                    }

                case ExitPointNode pointNode:
                    {
                        thisVertex = new ExitPoint(pointNode.label);
                        
                        if (diagramNode.children.Count > 0)
                        {
                            throw new DiagramNodeException(diagramNode, $"exit points cannot have children. See https://github.com/StateSmith/StateSmith/issues/3");
                        }
                        break;
                    }

                case ChoiceNode pointNode:
                    {
                        thisVertex = new ChoicePoint(pointNode.label);

                        if (diagramNode.children.Count > 0)
                        {
                            throw new DiagramNodeException(diagramNode, $"choice points cannot have children.");
                        }
                        break;
                    }
            }

            thisVertex.DiagramId = diagramNode.id;
            diagramVertexMap.Add(diagramNode, thisVertex);

            if (parentVertex != null)
            {
                parentVertex.AddChild(thisVertex);
            }

            if (visitChildren)
            {
                foreach (var child in diagramNode.children)
                {
                    ProcessNode(child, thisVertex);
                }
            }

            return thisVertex;
        }

        private static void PrintAndThrowIfNodeParseFail(Input.DiagramNode diagramNode, Vertex? parentVertex, LabelParser labelParser)
        {
            if (labelParser.HasError())
            {
                string reasons = ParserErrorsToReasonStrings(labelParser.GetErrors(), separator: "\n           ");

                string parentPath = VertexPathDescriber.Describe(parentVertex);
                string fullMessage = $@"Failed parsing node label
Parent path: `{parentPath}`
Label: `{diagramNode.label}`
Diagram id: `{diagramNode.id}`
Reason(s): {reasons}
";
                throw new ArgumentException(fullMessage);
            }
        }

        // todolow move out of compiler
        public static string ParserErrorsToReasonStrings(List<Error> errors, string separator)
        {
            var reasons = "";
            var needsSeparator = false;
            foreach (var error in errors)
            {
                if (needsSeparator)
                {
                    reasons += separator;
                }
                reasons += error.BuildMessage();
                needsSeparator = true;
            }

            return reasons;
        }

        private void ConvertBehaviors(Vertex vertex, StateNode stateNode)
        {
            foreach (var nodeBehavior in stateNode.behaviors)
            {
                Behavior behavior = ConvertBehavior(vertex, nodeBehavior);
                vertex.AddBehavior(behavior);
            }
        }

        private static Behavior ConvertBehavior(Vertex owningVertex, NodeBehavior nodeBehavior, Vertex? targetVertex = null)
        {
            var behavior = new Behavior(owningVertex: owningVertex, transitionTarget: targetVertex)
            {
                DiagramId = owningVertex.DiagramId, // may be overwritten for edges
                actionCode = nodeBehavior.actionCode ?? "",
                guardCode = nodeBehavior.guardCode ?? "",
                triggers = nodeBehavior.triggers,
                viaEntry = nodeBehavior.viaEntry,
                viaExit = nodeBehavior.viaExit,
            };

            if (nodeBehavior.order != null)
            {
                behavior.order = Double.Parse(nodeBehavior.order);
            }

            return behavior;
        }

        private static void SetStateFromStateNode(StateNode stateNode, State state)
        {
            state.nameIsGloballyUnique = stateNode.stateNameIsGlobal;
        }
    }
}
