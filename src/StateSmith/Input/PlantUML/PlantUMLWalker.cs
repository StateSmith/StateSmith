#nullable enable

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using StateSmith.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateSmith.Input.PlantUML;

public class PlantUMLWalker : PlantUMLBaseListener
{
    public List<DiagramEdge> edges = new();
    public DiagramNode root = new();
    public Dictionary<DiagramNode, string> nodeStereoTypeLookup = new();
    public Dictionary<string, DiagramNode> nodeMap = new();
    public List<KeptComment> keptCommentBlocks = new();

    private DiagramNode currentNode;

    // used to check if a state already contains an initial state
    private Dictionary<DiagramNode, DiagramNode> nodeInitialStateMap = new();

    // used to check if a state already contains a history state
    private Dictionary<DiagramNode, DiagramNode> nodeHistoryStateMap = new();

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
        currentNode = currentNode.parent.ThrowIfNull();
    }

    private DiagramNode GetCurrentNode()
    {
        return currentNode;
    }

    private DiagramNode GetOrAddHistoryState(PlantUMLParser.History_stateContext historyStateContext, DiagramNode parentNode)
    {
        if (!nodeHistoryStateMap.TryGetValue(parentNode, out var historyState))
        {
            historyState = new DiagramNode
            {
                label = SmGraph.VertexParseStrings.HistoryStateString,
                id = MakeId(historyStateContext)
            };
            nodeHistoryStateMap.Add(parentNode, historyState);
            AddNode(parentNode: parentNode, historyState);
        }

        return historyState;
    }

    private DiagramNode GetOrAddInitialState(PlantUMLParser.Start_end_stateContext startEndContext)
    {
        if (!nodeInitialStateMap.TryGetValue(currentNode, out var initialState))
        {
            initialState = new DiagramNode
            {
                label = SmGraph.VertexParseStrings.InitialStateString,
                id = MakeId(startEndContext)
            };
            nodeInitialStateMap.Add(currentNode, initialState);
            AddNode(parentNode: GetCurrentNode(), initialState);
        }

        return initialState;
    }

    private static string MakeId(ParserRuleContext vertexContext)
    {
        return $"line_{vertexContext.Start.Line}_column_{vertexContext.Start.Column}";
    }

    private void AddNode(DiagramNode parentNode, DiagramNode state)
    {
        parentNode.children.Add(state);
        state.parent = parentNode;
    }

    /// <summary>
    /// </summary>
    /// <param name="pumlId">PlantUML element ID. Must be unique within diagram.</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private DiagramNode GetOrAddNode(string pumlId, ParserRuleContext context)
    {
        if (!nodeMap.TryGetValue(pumlId, out var state))
        {
            state = new DiagramNode
            {
                label = pumlId,
                id = pumlId,
            };
            nodeMap.Add(pumlId, state);
            AddNode(parentNode: GetCurrentNode(), state);
        }

        return state;
    }

    public override void EnterDiagram([NotNull] PlantUMLParser.DiagramContext context)
    {
        root.id = context.startuml().statemachine_name()?.GetText() ?? "";

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
        (
            id: MakeId(context),
            source: source,
            target: destination,
            label: label
        );

        if (destination.label == SmGraph.VertexParseStrings.InitialStateString)
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

    /// <summary>
    /// plantuml has a number of special escape sequences. See also unit tests.
    /// https://github.com/StateSmith/StateSmith/issues/369
    /// </summary>
    internal static string Decode(string? escapedString)
    {
        if (escapedString == null)
        {
            return "";
        }

        escapedString = escapedString.Trim();
        StringBuilder sb = new();

        bool escapeStarted = false;
        for (int i = 0; i < escapedString.Length; i++)
        {
            char c = escapedString[i];

            if (escapeStarted)
            {
                switch (c)
                {
                    case 'n': sb.Append('\n');  break;
                    case 't': sb.Append('\t');  break;
                    case '\\': sb.Append('\\'); break;

                    // left/right alignment codes appear as new lines in plantuml
                    // https://github.com/StateSmith/StateSmith/issues/362
                    case 'l':                        
                    case 'r':
                        //sb.Append('\n'); // I don't think we actually want new lines here. Might cause StateSmith grammar to fail. Would need some testing.
                        break;

                    default:
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                }
                escapeStarted = false;
            }
            else
            {
                if (c == '\\')
                {
                    escapeStarted = true;
                }
                else
                {
                    sb.Append(c);
                }
            }
        }

        // Note! if last character is a backslash, it isn't shown in the diagram. It is actually a line continuation character, but our grammar doesn't support that yet.
        // For now, we just ignore it. https://github.com/StateSmith/StateSmith/issues/379

        return sb.ToString();
    }

    private static void ThrowValidationFailure(string message, global::Antlr4.Runtime.ParserRuleContext context)
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
            node = GetOrAddInitialState(vertexContext.start_end_state());
        }
        else if(vertexContext.history_state() != null)
        {
            node = GetOrAddHistoryState(vertexContext.history_state(), GetCurrentNode());
        }
        else
        {
            PlantUMLParser.State_idContext state_id = vertexContext.state_id();
            node = GetOrAddNode(state_id.identifier().GetText(), vertexContext.state_id());
            
            if (state_id.history_state() != null)
            {
                node = GetOrAddHistoryState(state_id.history_state(), node);
            }
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

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/335
    /// </summary>
    public override void EnterKept_block_comment([NotNull] PlantUMLParser.Kept_block_commentContext context)
    {
        const int startCharsToSkip = 3; // skip at start: /'!
        const int endCharsToSkip = 2;   // skip at end  : '/

        string commentContents = context.KEPT_BLOCK_COMMENT().GetText();
        commentContents = commentContents.Substring(startCharsToSkip, commentContents.Length - (startCharsToSkip + endCharsToSkip));
        KeptComment keptCommentBlock = new(diagramId: MakeId(context), comment: commentContents);
        keptCommentBlocks.Add(keptCommentBlock);
    }
}
