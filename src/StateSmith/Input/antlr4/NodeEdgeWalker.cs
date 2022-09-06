using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using StateSmith.Input.Expansions;
using System.Collections.Generic;

namespace StateSmith.Input.antlr4
{
    public class NodeEdgeWalker : Grammar1BaseListener
    {
        public Node node;
        public StateNode stateNode;
        public OrthoStateNode orthoStateNode;

        /// <summary>
        /// done separately for parsing edges as well
        /// </summary>
        NodeBehavior currentBehavior;
        public List<NodeBehavior> behaviors = new List<NodeBehavior>();

        public override void EnterStatemachine_defn([NotNull] Grammar1Parser.Statemachine_defnContext context)
        {
            var stateMachineNode = new StateMachineNode();
            stateMachineNode.name = context.IDENTIFIER().GetText();
            node = stateMachineNode;
        }

        public override void EnterState_defn([NotNull] Grammar1Parser.State_defnContext context)
        {
            stateNode = new StateNode();
            node = stateNode;
        }

        public override void EnterOrtho_defn([NotNull] Grammar1Parser.Ortho_defnContext context)
        {
            orthoStateNode = new OrthoStateNode();
            node = orthoStateNode;
            stateNode = orthoStateNode;

            orthoStateNode.order = context.ortho_order()?.number()?.GetText();
        }

        public override void EnterNotes_node([NotNull] Grammar1Parser.Notes_nodeContext context)
        {
            var notesNode = new NotesNode();
            notesNode.notes = context.notes_text()?.GetText()?.Trim();
            node = notesNode;
        }

        //---------------------------

        public override void EnterState_id([NotNull] Grammar1Parser.State_idContext context)
        {
            string stateName;
            if (context.global_id() == null)
            {
                stateName = context.IDENTIFIER().GetText();
            }
            else
            {
                stateName = context.global_id().IDENTIFIER().GetText();
                stateNode.stateNameIsGlobal = true;
            }

            stateNode.stateName = stateName;
        }


        public override void EnterBehavior([NotNull] Grammar1Parser.BehaviorContext context)
        {
            currentBehavior = new NodeBehavior();
            currentBehavior.order = context.order()?.number()?.GetText();
            currentBehavior.guardCode = context.guard()?.guard_code()?.GetText().Trim();
            currentBehavior.actionCode = GetActionCodeText(context.action()?.action_code());
            behaviors.Add(currentBehavior);
        }

        private string GetActionCodeText(Grammar1Parser.Action_codeContext action_codeContext)
        {
            if (action_codeContext == null || action_codeContext.ChildCount == 0)
            {
                return null;
            }

            var code = TryGetBracedActionCode(action_codeContext) ?? action_codeContext.GetText();

            return code;
        }


        private string TryGetBracedActionCode(Grammar1Parser.Action_codeContext action_codeContext)
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

        public override void EnterTrigger_id([NotNull] Grammar1Parser.Trigger_idContext context)
        {
            currentBehavior.triggers.Add(context.expandable_identifier().GetText());
        }

        public override void ExitBehavior([NotNull] Grammar1Parser.BehaviorContext context)
        {
            currentBehavior = null;
        }
    }
}
