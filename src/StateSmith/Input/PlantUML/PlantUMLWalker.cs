using Antlr4.Runtime;
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
            string pumlId = context.identifier().GetText();
            string label = pumlId;

            if (context.STRING() != null)
            {
                label = Decode(context.STRING().GetText());
                label = label.Substring(1, label.Length - 2);   // remove quotes
            }

            var node = GetOrAddNode(pumlId, context.identifier());
            HandleStereotype(node, context.stereotype());
            currentNode = node;
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
                    id = MakeId(vertexContext.start_end_state())
                };
                nodeInitalStateMap.Add(currentNode, initialState);
                AddNode(initialState);
            }

            return initialState;
        }

        private static string MakeId(ParserRuleContext vertexContext)
        {
            return $"line_{vertexContext.Start.Line}_column_{vertexContext.Start.Column}";
        }

        private void AddNode(DiagramNode state)
        {
            currentNode.children.Add(state);
            state.parent = currentNode;
        }

        private DiagramNode GetOrAddNode(string pumlId, ParserRuleContext context)
        {
            if (!nodeMap.TryGetValue(pumlId, out var state))
            {
                state = new DiagramNode
                {
                    label = pumlId,
                    id = MakeId(context)
                };
                nodeMap.Add(pumlId, state);
                AddNode(state);
            }

            return state;
        }

        public override void EnterDiagram([NotNull] PlantUMLParser.DiagramContext context)
        {
            root.id = context.startuml().identifier()?.GetText() ?? "";

            if (root.id == "")
            {
                ThrowValidationFailure("PlantUML diagrams need a name and should start like `@startuml MySmName`", context.startuml());
            }

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
                label = label,
                id = MakeId(context)
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
            var state = GetOrAddNode(context.identifier().GetText(), context.identifier());
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

        private static void ThrowValidationFailure(string message, Antlr4.Runtime.ParserRuleContext context)
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
                node = GetOrAddNode(vertexContext.state_id().GetText(), vertexContext.state_id());
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
