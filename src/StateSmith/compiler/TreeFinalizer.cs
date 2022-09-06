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
using StateSmith.Common;
using StateSmith.compiler.Visitors;

#nullable enable

namespace StateSmith.Compiling
{
    public class TreeFinalizer : VertexWalker
    {
        private Statemachine statemachine = new Statemachine("dummy_to_prevent_null");
        //private Stack<Statemachine> statemachines = new Stack<Statemachine>();    //todolow https://github.com/adamfk/StateSmith/issues/7

        private void ProcessBehaviors(Vertex v)
        {
            foreach (var behavior in v.Behaviors)
            {
                foreach (var trigger in behavior.triggers)
                {
                    TriggerHelper.MaybeAddTrigger(statemachine, behavior, trigger);
                }
            }
        }


        public override void VertexEntered(Vertex v)
        {
            if (v is Statemachine sm)
            {
                statemachine = sm;
            }
            ProcessBehaviors(v);
        }

        public override void VertexExited(Vertex v)
        {
            if (v is Statemachine _)
            {
                statemachine = new Statemachine("dummy_to_prevent_null");   //todolow https://github.com/adamfk/StateSmith/issues/7
                //Don't worry about validation in this step
            }
        }

        internal void FinalizeTree(Vertex v)
        {
            Walk(v);
        }
    }
}
