using System;
using Antlr4.Runtime.Misc;
using System.Collections.Generic;

namespace StateSmith.Input.Antlr4;

public class NodeEdgeWalker : StateSmithLabelGrammarBaseListener
{
    public Node node;
    public StateNode stateNode;
    public OrthoStateNode orthoStateNode;

    /// <summary>
    /// done separately for parsing edges as well
    /// </summary>
    NodeBehavior currentBehavior;
    public List<NodeBehavior> behaviors = new();

    public override void EnterStatemachine_defn([NotNull] StateSmithLabelGrammarParser.Statemachine_defnContext context)
    {
        var stateMachineNode = new StateMachineNode
        {
            name = context.statemachine_name().GetText()
        };
        node = stateMachineNode;
    }

    public override void EnterState_defn([NotNull] StateSmithLabelGrammarParser.State_defnContext context)
    {
        stateNode = new StateNode();
        node = stateNode;
    }

    public override void EnterOrtho_defn([NotNull] StateSmithLabelGrammarParser.Ortho_defnContext context)
    {
        orthoStateNode = new OrthoStateNode();
        node = orthoStateNode;
        stateNode = orthoStateNode;

        orthoStateNode.order = context.ortho_order()?.number()?.GetText();
    }

    public override void EnterNotes_node([NotNull] StateSmithLabelGrammarParser.Notes_nodeContext context)
    {
        var notesNode = new NotesNode
        {
            notes = context.any_text()?.GetText()?.Trim()
        };
        node = notesNode;
    }

    public override void EnterConfig_node([NotNull] StateSmithLabelGrammarParser.Config_nodeContext context)
    {
        node = new ConfigNode(name: context.SS_IDENTIFIER().GetText(), value: context.any_text()?.GetText() ?? "");
    }

    public override void EnterEntry_point([NotNull] StateSmithLabelGrammarParser.Entry_pointContext context)
    {
        var n = new EntryPointNode
        {
            label = context.point_label().GetText()
        };
        node = n;
    }

    public override void EnterExit_point([NotNull] StateSmithLabelGrammarParser.Exit_pointContext context)
    {
        var n = new ExitPointNode
        {
            label = context.point_label().GetText()
        };
        node = n;
    }

    public override void EnterChoice([NotNull] StateSmithLabelGrammarParser.ChoiceContext context)
    {
        var n = new ChoiceNode
        {
            label = context.point_label()?.GetText() ?? ""
        };
        node = n;
    }

    //---------------------------

    public override void EnterState_id([NotNull] StateSmithLabelGrammarParser.State_idContext context)
    {
        string stateName;
        if (context.global_id() == null)
        {
            stateName = context.SS_IDENTIFIER().GetText();
        }
        else
        {
            stateName = context.global_id().SS_IDENTIFIER().GetText();
            stateNode.stateNameIsGlobal = true;
        }

        stateNode.stateName = stateName;
    }


    public override void EnterBehavior([NotNull] StateSmithLabelGrammarParser.BehaviorContext context)
    {
        currentBehavior = new NodeBehavior
        {
            order = context.order()?.number()?.GetText(),
            guardCode = context.guard()?.guard_code()?.GetText().Trim(),
            actionCode = GetActionCodeText(context.action()?.action_code())
        };
        AddAnyVias(context);

        behaviors.Add(currentBehavior);
    }

    private void AddAnyVias(StateSmithLabelGrammarParser.BehaviorContext context)
    {
        StateSmithLabelGrammarParser.Transition_viaContext[] vias = context.transition_vias()?.transition_via();

        if (vias == null)
        {
            return;
        }

        foreach (var via in vias)
        {
            if (via.via_entry_type() != null)
            {
                if (currentBehavior.viaEntry != null)
                {
                    throw new ArgumentException("can't have multiple `via entry` statements");  // todolow throw as more specific exception type with more info
                }
                currentBehavior.viaEntry = via.point_label().GetText();
            }

            if (via.via_exit_type() != null)
            {
                if (currentBehavior.viaExit != null)
                {
                    throw new ArgumentException("can't have multiple `via exit` statements");  // todolow throw as more specific exception type with more info
                }
                currentBehavior.viaExit = via.point_label().GetText();
            }
        }
    }

    private string GetActionCodeText(StateSmithLabelGrammarParser.Action_codeContext action_codeContext)
    {
        if (action_codeContext == null || action_codeContext.ChildCount == 0)
        {
            return null;
        }

        var code = TryGetBracedActionCode(action_codeContext) ?? action_codeContext.GetText();

        return code;
    }


    private string TryGetBracedActionCode(StateSmithLabelGrammarParser.Action_codeContext action_codeContext)
    {
        var any_code = action_codeContext.braced_expression()?.any_code();

        if (any_code == null)
        {
            return null;
        }

        var visitor = new DeIndentVisitor();

        foreach (var item in any_code.code_element())
        {
            visitor.Visit(item);
        }

        return visitor.stringBuilder.ToString().Trim();
    }

    public override void EnterTrigger_id([NotNull] StateSmithLabelGrammarParser.Trigger_idContext context)
    {
        currentBehavior.triggers.Add(context.expandable_identifier().GetText());
    }

    public override void ExitBehavior([NotNull] StateSmithLabelGrammarParser.BehaviorContext context)
    {
        currentBehavior = null;
    }
}
