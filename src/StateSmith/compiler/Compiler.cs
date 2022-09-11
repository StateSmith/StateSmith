using StateSmith.Input.antlr4;
using StateSmith.Input.Expansions;
using StateSmith.Input.Yed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using StateSmith.compiler;
using StateSmith.Input;
using System.IO;
using StateSmith.compiler.Visitors;

namespace StateSmith.Compiling
{
    public class Compiler
    {
        public List<Vertex> rootVertices = new List<Vertex>();
        private List<string> eventNames = new List<string>();
        private Dictionary<Input.DiagramNode, Vertex> diagramVertexMap = new Dictionary<Input.DiagramNode, Vertex>();

        public void CompileFile(string filepath)
        {
            YedParser yedParser = new YedParser();

            yedParser.Parse(filepath);

            CompileDiagramNodesEdges(yedParser.GetRootNodes(), yedParser.GetEdges());
        }

        public void CompileDiagramNodesEdges(List<DiagramNode> rootNodes, List<DiagramEdge> edges)
        {
            foreach (var node in rootNodes)
            {
                rootVertices.Add(ProcessNode(node, parentVertex: null));
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
                    SetupDescendants(namedVertex);
                }
            }
        }

        /// <summary>
        /// Don't do any modifications to trees after this is called
        /// </summary>
        public void FinalizeTrees()
        {
            var finalizer = new TreeFinalizer();

            foreach (var v in rootVertices)
            {
                finalizer.FinalizeTree(v);
            }
        }

        public void Validate()
        {
            foreach (var v in rootVertices)
            {
                var validator = new VertexValidator();
                v.Accept(validator);
            }
        }

        public void SimplifyInitialStates()
        {
            foreach (var v in rootVertices)
            {
                var processor = new InitialStateProcessor();
                v.Accept(processor);
            }
        }

        // https://github.com/StateSmith/StateSmith/issues/2
        public void SupportParentAlias()
        {
            foreach (var v in rootVertices)
            {
                var processor = new ParentAliasStateProcessor();
                v.Accept(processor);
            }
        }

        public void DefaultToDoEventIfNoTrigger()
        {
            foreach (var v in rootVertices)
            {
                var processor = new DefaultToDoEventVisitor();
                v.Accept(processor);
            }
        }

        public void AddEventName(string eventName)
        {
            // Don't worry about O(N) look up cost for now as we don't expect many events.
            if (eventNames.Contains(eventName) == false)
            {
                eventNames.Add(eventName);
            }
        }

        public List<NamedVertex> GetNamedVertices(string name)
        {
            return rootVertices.Descendants(name);
        }

        public NamedVertex GetVertex(string name)
        {
            return rootVertices.Descendant(name);
        }
        

        private void ProcessEdge(Input.DiagramEdge edge)
        {
            try
            {
                ProcessEdgeInner(edge);
            }
            catch (Exception ex)
            {
                DiagramEdgeException diagramEdgeException = new DiagramEdgeException(edge, $"Failed while converting {nameof(DiagramEdge)} to state transition", ex);

                //TODO low move this functionality to StateSmith runner/cli or something
                StringBuilder sb = new StringBuilder();
                sb.Append(diagramEdgeException);
                sb.Append("\n\n");
                edge.Describe(sb);
                sb.Append("\n==========================\n");
                sb.Append("EDGE SOURCE NODE:\n");
                DiagramNode.FullyDescribe(edge.source, sb);
                sb.Append("\n==========================\n");
                sb.Append("EDGE TARGET NODE:\n");
                DiagramNode.FullyDescribe(edge.target, sb);

                string dumpInfo = sb.ToString();
                File.WriteAllText("dump_info.txt", dumpInfo);

                throw diagramEdgeException;
            }
        }

        private Vertex GetVertexFromNode(DiagramNode node)
        {
            if (NodeHasState(node))
            {
                return diagramVertexMap[node];
            }

            throw new DiagramNodeException(node, $"Could not find State for {nameof(DiagramNode)} with id `{node.id}`. Is it inside a $NOTES block? See https://TODO");
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
            if (EdgeNodesInsideNotesSection(edge))
            {
                return;
            }

            var sourceVertex = GetVertexFromNode(edge.source);
            var targetVertex = GetVertexFromNode(edge.target);

            LabelParser labelParser = new LabelParser();
            List<NodeBehavior> nodeBehaviors = labelParser.ParseEdgeLabel(edge.label);

            PrintAndThrowIfEdgeParseFail(edge, sourceVertex, targetVertex, labelParser);

            if (nodeBehaviors.Count == 0)
            {
                sourceVertex.AddBehavior(new Behavior(owningVertex: sourceVertex, transitionTarget: targetVertex));
            }

            foreach (var nodeBehavior in nodeBehaviors)
            {
                var behavior = ConvertBehavior(owningVertex: sourceVertex, targetVertex: targetVertex, nodeBehavior: nodeBehavior);
                sourceVertex.AddBehavior(behavior);
            }
        }

        private bool NodeInsideNotesVertex(DiagramNode node)
        {
            while (node != null)
            {
                Vertex v = TryGetVertexFromNode(node);
                if (v is NotesVertex _)
                {
                    return true;
                }
                node = node.parent;
            }

            return false;
        }

        /// <summary>
        /// This will be the case when there are "commented out" edges inside a $NOTES block
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        private bool EdgeNodesInsideNotesSection(DiagramEdge edge)
        {
            return NodeInsideNotesVertex(edge.source) && NodeInsideNotesVertex(edge.target);
        }

        private static void PrintAndThrowIfEdgeParseFail(Input.DiagramEdge edge, Vertex sourceVertex, Vertex targetVertex, LabelParser labelParser)
        {
            if (labelParser.HasError())
            {
                string fromString = VertexPathDescriber.Describe(sourceVertex);
                string toString = VertexPathDescriber.Describe(targetVertex);
                string fullMessage = $"Failed parsing edge from `{fromString}` to `{toString}`. Diagram id:{edge.id}.\n";
                foreach (var error in labelParser.GetErrors())
                {
                    fullMessage += error.BuildMessage() + "\n";
                }

                Console.WriteLine(fullMessage);

                throw new ArgumentException(fullMessage);
            }
        }

        private static void VisitVertices<T>(Vertex vertex, Action<T> action) where T : Vertex
        {
            if (typeof(T).IsAssignableFrom(vertex.GetType()))
            {
                action((T)vertex);
            }

            foreach (var child in vertex.Children)
            {
                VisitVertices<T>(child, action);
            }
        }

        private static void SetupDescendants(NamedVertex parentVertex)
        {
            VisitVertices<NamedVertex>(parentVertex, vertex => {
                //add this vertex to ancestors
                var parent = vertex.Parent;
                while (parent != null)
                {
                    parent._namedDescendants.AddIfMissing(vertex.Name, vertex);
                    parent = parent.Parent;
                }
            });
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
                VisitVertices<Vertex>(root, vertex => {
                    foreach (var behavior in vertex.Behaviors)
                    {
                        ExpandBehavior(expander, behavior);
                    }
                });
            }
        }

        private Vertex ProcessNode(Input.DiagramNode diagramNode, Vertex parentVertex)
        {
            if (diagramNode.label == null || diagramNode.label.Trim() == "")
            {
                return null;
            }

            LabelParser labelParser = new LabelParser();
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
                        visitChildren = false;
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
                            if (string.Equals(stateNode.stateName, "$initial_state", StringComparison.OrdinalIgnoreCase))
                            {
                                thisVertex = new InitialState();
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

        private static void PrintAndThrowIfNodeParseFail(Input.DiagramNode diagramNode, Vertex parentVertex, LabelParser labelParser)
        {
            if (labelParser.HasError())
            {
                string parentPath = VertexPathDescriber.Describe(parentVertex);
                string fullMessage = $"Failed parsing node label. Parent path: `{parentPath}`.\n<label>\n{diagramNode.label}\n</label>\nDiagram id:`{diagramNode.id}`.\n";
                foreach (var error in labelParser.GetErrors())
                {
                    fullMessage += error.BuildMessage() + "\n";
                }

                Console.WriteLine(fullMessage);

                throw new ArgumentException(fullMessage);
            }
        }

        private void ConvertBehaviors(Vertex vertex, StateNode stateNode)
        {
            foreach (var nodeBehavior in stateNode.behaviors)
            {
                Behavior behavior = ConvertBehavior(vertex, nodeBehavior);
                vertex.AddBehavior(behavior);
            }
        }

        private static Behavior ConvertBehavior(Vertex owningVertex, NodeBehavior nodeBehavior, Vertex targetVertex = null)
        {
            var behavior = new Behavior(owningVertex: owningVertex, transitionTarget: targetVertex)
            {
                actionCode = nodeBehavior.actionCode,
                guardCode = nodeBehavior.guardCode,
                triggers = nodeBehavior.triggers
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
