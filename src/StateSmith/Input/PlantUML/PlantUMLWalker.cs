using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;

#nullable enable

namespace StateSmith.Input.PlantUML
{
    public class PlantUMLWalker : PlantUMLBaseListener
    {
        public List<DiagramEdge> edges = new();
        public DiagramNode root = new();

        private DiagramNode currentNode;

        private Dictionary<DiagramNode, DiagramNode> nodeInitalStateMap = new();
        public Dictionary<DiagramNode, string> nodeStereoTypeLookup = new();
        public Dictionary<string, DiagramNode> nodeMap = new();


        public PlantUMLWalker()
        {
            currentNode = root;
        }

        public override void EnterState_explicit([NotNull] PlantUMLParser.State_explicitContext context)
        {
            var id = context.identifier().GetText();

            string label = id;

            if (context.STRING() != null)
            {
                label = Decode(context.STRING().GetText());
                label = label.Substring(1, label.Length - 2);   // remove quotes
            }

            var node = GetOrAddNode(id);
            HandleStereotype(node, context.stereotype());
            currentNode = node;
            node.id = id;
            node.label = label;
        }

        public override void ExitState_explicit([NotNull] PlantUMLParser.State_explicitContext context)
        {
            currentNode = currentNode.parent;
        }

        private DiagramNode GetOrAddInitialState(PlantUMLParser.VertexContext vertexContext)
        {
            if (!nodeInitalStateMap.TryGetValue(currentNode, out var initialState))
            {
                initialState = new DiagramNode
                {
                    label = Compiling.Compiler.InitialStateString,
                    id = vertexContext.start_end_state().Start.Line + "_" + vertexContext.start_end_state().Start.Column
                };
                nodeInitalStateMap.Add(currentNode, initialState);
                AddNode(initialState);
            }

            return initialState;
        }

        private void AddNode(DiagramNode state)
        {
            currentNode.children.Add(state);
            state.parent = currentNode;
        }

        private DiagramNode GetOrAddNode(string id)
        {
            if (!nodeMap.TryGetValue(id, out var state))
            {
                state = new DiagramNode
                {
                    label = id,
                    id = id
                };
                nodeMap.Add(id, state);
                AddNode(state);
            }

            return state;
        }

        public override void EnterDiagram([NotNull] PlantUMLParser.DiagramContext context)
        {
            root.id = context.startuml().identifier()?.GetText() ?? "ROOT";
            root.label = "$STATEMACHINE : " + root.id;
        }

        public override void EnterTransition([NotNull] PlantUMLParser.TransitionContext context)
        {
            DiagramNode source = VertexToNode(context.vertex(0));
            DiagramNode destination = VertexToNode(context.vertex(1));
            var label = Decode(context.transition_event_guard_code()?.GetText());

            var edge = new DiagramEdge
            {
                source = source,
                target = destination,
                label = label
            };

            if (destination.label == Compiling.Compiler.InitialStateString)
            {
                ThrowValidationFailure("StateSmith doesn't support end states", context);
                return;
            }

            edges.Add(edge);
        }

        public override void EnterState_contents([NotNull] PlantUMLParser.State_contentsContext context)
        {
            var state = GetOrAddNode(context.identifier().GetText());
            state.label += "\n" + Decode(context.rest_of_line().GetText());
        }

        private string Decode(string? escapedString)
        {
            if (escapedString == null)
            {
                return "";
            }

            return escapedString.Trim().Replace(@"\n", "\n");
        }

        private static void ThrowValidationFailure(string message, PlantUMLParser.TransitionContext context)
        {
            int line = context.Start.Line;
            int column = context.Start.Column;
            throw new InvalidOperationException($"{message}. Location Details {{ line: {line}, column: {column}, text: `{context.GetText()}`. }}"); //todolow throw a custom exception type
        }

        private DiagramNode VertexToNode(PlantUMLParser.VertexContext vertexContext)
        {
            DiagramNode node;

            if (vertexContext.start_end_state() != null)
            {
                node = GetOrAddInitialState(vertexContext);
            }
            else
            {
                node = GetOrAddNode(vertexContext.state_id().GetText());
            }

            HandleStereotype(node, vertexContext.stereotype());

            return node;
        }

        private void HandleStereotype(DiagramNode node, PlantUMLParser.StereotypeContext stereotypeContext)
        {
            if (stereotypeContext == null)
            {
                return;
            }

            string newStereotype = stereotypeContext.identifier().GetText();

            if (nodeStereoTypeLookup.TryGetValue(node, out var existingStereotype))
            {
                if (existingStereotype != newStereotype)
                {
                    throw new InvalidOperationException($"node stereotype redefined from `{existingStereotype}` to `{newStereotype}` ");  //todolow throw a custom exception type
                }
            }

            nodeStereoTypeLookup.Add(node, newStereotype);
        }
    }


}
