using StateSmith.SmGraph;
using System;
using System.Text.RegularExpressions;

namespace StateSmith.output.C99BalancedCoder1
{
    public class CNameMangler
    {
        private StateMachine sm = new StateMachine("dummy");

        public CNameMangler()
        {
            
        }

        public CNameMangler(StateMachine sm)
        {
            this.sm = sm;
        }

        public void SetStateMachine(StateMachine sm)
        {
            if (sm == null)
            {
                throw new ArgumentNullException("sm");
            }

            this.sm = sm;
        }

        public virtual string SmName => sm.Name;

        public virtual string CFileName => $"{SmName}.c";
        public virtual string HFileName => $"{SmName}.h";

        //--------------------------------------------------------
        //--------------------------------------------------------
        //--------------------------------------------------------


        public virtual string SmEventEnum => $"{SmName}_EventId";

        /// <summary>
        /// Set to `__attribute__((packed)) ` for gcc if you want smallest possible enum
        /// </summary>
        public virtual string SmEventEnumAttribute => $"";

        public virtual string SmEventEnumValue(string evt) => $"{SmEventEnum}_{evt.ToUpper()}";

        public virtual string SmEventEnumCount => $"{SmEventEnum}Count";

        //--------------------------------------------------------
        //--------------------------------------------------------
        //--------------------------------------------------------


        #region StateEnum

        public virtual string SmStateEnum => $"{SmName}_StateId";

        /// <summary>
        /// Set to `__attribute__((packed)) ` for gcc if you want smallest possible enum
        /// </summary>
        public virtual string SmStateEnumAttribute => $"";

        public virtual string SmStateEnumValue(NamedVertex namedVertex)
        {
            string stateName = SmStateName(namedVertex);
            return $"{SmStateEnum}_{stateName.ToUpper()}";
        }

        public virtual string SmStateName(NamedVertex namedVertex)
        {
            return namedVertex.Parent == null ? "ROOT" : namedVertex.Name.ToUpper();
        }

        public virtual string SmStateEnumCount => $"{SmStateEnum}Count";

        #endregion StateEnum

        //--------------------------------------------------------
        //--------------------------------------------------------
        //--------------------------------------------------------

        public virtual string SmStructName => $"{SmName}";

        public virtual string SmStructTypedefName => SmStructName;

        public virtual string SmFuncTypedef => $"{SmStructName}_Func";

        /// <summary>
        /// Ctor is short for constructor
        /// </summary>
        public virtual string SmFuncCtor => $"{SmName}_ctor";
        public virtual string SmFuncStart => $"{SmName}_start";
        public virtual string SmFuncDispatchEvent => $"{SmName}_dispatch_event";

        public virtual string SmFuncExitUpTo => $"exit_up_to_state_handler";

        public virtual string SmFuncToString => $"{SmName}_state_id_to_string";
        public virtual string SmStateToString(NamedVertex state) => $"{SmStateName(state).ToUpper()}";

        public virtual string HistoryVarName(HistoryVertex historyVertex) => $"{historyVertex.ParentState.Name}_history";
        public virtual string HistoryVarEnumName(HistoryVertex historyVertex) => $"{SmName}_{historyVertex.ParentState.Name}_HistoryId";
        public virtual string HistoryVarEnumValueName(HistoryVertex historyVertex, Vertex transitionTarget)
        {
            var description = Vertex.Describe(transitionTarget);
            description = Regex.Replace(description, @"[().]", "");
            var result = HistoryVarEnumName(historyVertex) + "__" + description;
            return result;
        }

        public virtual string SmFuncTriggerHandler(NamedVertex state, string triggerName)
        {
            return $"{SmStateName(state)}_{triggerName.ToLower()}";
        }
    }
}