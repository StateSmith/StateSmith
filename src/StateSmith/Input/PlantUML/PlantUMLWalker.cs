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

        private DiagramNode currentState;

        private Dictionary<DiagramNode, DiagramNode> stateInitalStateMap = new();

        public Dictionary<string, DiagramNode> nodeMap = new();


        public PlantUMLWalker()
        {
            currentState = root;
        }

        private DiagramNode GetOrAddInitialState(PlantUMLParser.VertexContext vertexContext)
        {
            if (!stateInitalStateMap.TryGetValue(currentState, out var initialState))
            {
                initialState = new DiagramNode
                {
                    label = Compiling.Compiler.InitialStateString,
                    id = vertexContext.start_end_state().Start.Line + ":" + vertexContext.start_end_state().Start.Column
                };
                stateInitalStateMap.Add(currentState, initialState);
                currentState.children.Add(initialState);
            }

            return initialState;
        }

        private DiagramNode GetOrAddState(string id)
        {
            if (!nodeMap.TryGetValue(id, out var state))
            {
                state = new DiagramNode
                {
                    label = id,
                    id = id
                };
                nodeMap.Add(id, state);
                currentState.children.Add(state);
            }

            return state;
        }

        public override void EnterDiagram([NotNull] PlantUMLParser.DiagramContext context)
        {
            root.id = context.startuml().identifier()?.GetText() ?? "ROOT";
        }

        public override void EnterTransition([NotNull] PlantUMLParser.TransitionContext context)
        {
            DiagramNode source = VertexToNode(context.vertex(0));
            DiagramNode destination = VertexToNode(context.vertex(1));
            var label = context.transition_event_guard_code()?.GetText().Trim() ?? "";

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
            var state = GetOrAddState(context.identifier().GetText());
            state.label += "\n" + context.rest_of_line().GetText().Trim();
        }

        private static void ThrowValidationFailure(string message, PlantUMLParser.TransitionContext context)
        {
            int line = context.Start.Line;
            int column = context.Start.Column;
            throw new ArgumentException($"{message}. Location Details {{ line: {line}, column: {column}, text: `{context.GetText()}`. }}");
        }

        private DiagramNode VertexToNode(PlantUMLParser.VertexContext vertexContext)
        {
            if (vertexContext.start_end_state() != null)
            {
                return GetOrAddInitialState(vertexContext);
            }
            else
            {
                return GetOrAddState(vertexContext.state_id().GetText());
            }
        }
    }


}
