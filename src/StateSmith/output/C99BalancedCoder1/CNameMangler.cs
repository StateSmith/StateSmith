using StateSmith.Compiling;
using System;

namespace StateSmith.output.C99BalancedCoder1
{
    public class CNameMangler
    {
        private Statemachine sm = new Statemachine("dummy");

        public CNameMangler()
        {
            
        }

        public CNameMangler(Statemachine sm)
        {
            this.sm = sm;
        }

        public void SetStateMachine(Statemachine sm)
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


        public virtual string SmFuncToString => $"{SmName}_state_id_to_string";
        public virtual string SmStateToString(NamedVertex state) => $"{SmStateName(state).ToUpper()}";


        public virtual string SmFuncTriggerHandler(NamedVertex state, string triggerName)
        {
            return $"{SmStateName(state)}_{triggerName.ToLower()}";
        }
    }
}