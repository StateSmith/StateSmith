using StateSmith.SmGraph;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using StateSmith.Common;

namespace StateSmith.output.C99BalancedCoder1
{
    public class CEnumBuilder
    {
        readonly CodeGenContext ctx;
        readonly CNameMangler mangler;

        public CEnumBuilder(CodeGenContext ctx)
        {
            this.ctx = ctx;
            mangler = ctx.mangler;
        }

        public void OutputEventIdCode()
        {
            OutputFile file = new(ctx, ctx.hFileSb);
            string smName = ctx.sm.Name;

            file.Append($"enum {mangler.SmEventEnumAttribute}{mangler.SmEventEnum}");
            file.StartCodeBlock();
            List<string> nonDoEvents = GetNonDoEvents(out var hadDoEvent);

            int enumOffset = 0;

            if (hadDoEvent)
            {
                enumOffset = 1;
                file.AppendLine($"{mangler.SmEventEnumValue(TriggerHelper.TRIGGER_DO)} = 0, // The `do` event is special. State event handlers do not consume this event (ancestors all get it too) unless a transition occurs.");
            }

            for (int i = 0; i < nonDoEvents.Count; i++)
            {
                string evt = nonDoEvents[i];
                file.AppendLine($"{mangler.SmEventEnumValue(evt)} = {i + enumOffset},");
            }

            file.FinishCodeBlock(";");
            file.FinishLine();

            OutputEventIdCount(file, nonDoEvents.Count + enumOffset);
        }

        private List<string> GetNonDoEvents(out bool hadDoEvent)
        {
            var nonDoEvents = ctx.sm.GetEventListCopy();
            hadDoEvent = nonDoEvents.RemoveAll((e) => TriggerHelper.IsDoEvent(e)) > 0;
            return nonDoEvents;
        }

        protected void OutputEventIdCount(OutputFile file, int count)
        {
            var enumValueName = mangler.SmEventEnumCount;
            OutputAnonEnum(file, enumValueName, count);
        }

        public void OutputStateIdCode()
        {
            OutputFile file = new(ctx, ctx.hFileSb);
            string smName = ctx.sm.Name;

            file.Append($"enum {mangler.SmStateEnumAttribute}{mangler.SmStateEnum}");
            file.StartCodeBlock();

            var namedVertices = ctx.sm.GetNamedVerticesCopy();
            for (int i = 0; i < namedVertices.Count; i++)
            {
                NamedVertex namedVertex = namedVertices[i];
                file.AppendLine($"{mangler.SmStateEnumValue(namedVertex)} = {i},");
            }

            file.FinishCodeBlock(";");
            file.FinishLine();

            OutputStateIdCount(file, smName, namedVertices.Count);
        }

        protected void OutputStateIdCount(OutputFile file, string smName, int count)
        {
            var enumValueName = mangler.SmStateEnumCount;
            OutputAnonEnum(file, enumValueName, count);
        }

        public void OutputHistoryIdCode(HistoryVertex historyVertex)
        {
            OutputFile file = new(ctx, ctx.hFileSb);
            file.Append($"enum {mangler.SmStateEnumAttribute}{mangler.HistoryVarEnumName(historyVertex)}");
            file.StartCodeBlock();

            // default behavior is the last one
            Behavior defaultBehavior = historyVertex.Behaviors.Last();
            file.AppendLine($"{(mangler.HistoryVarEnumValueName(historyVertex, defaultBehavior.TransitionTarget))} = 0, // default transition");

            for (int i = 0; i < historyVertex.Behaviors.Count - 1; i++)
            {
                var b = historyVertex.Behaviors[i];
                file.AppendLine($"{mangler.HistoryVarEnumValueName(historyVertex, b.TransitionTarget)} = {i+1},");
            }

            file.FinishCodeBlock(";");
            file.FinishLine();
        }

        private static void OutputAnonEnum(OutputFile file, string enumValueName, int count)
        {
            // We put the count in a separate anonymous enum because it isn't really apart of the EventId enum values.
            // If we put it in there and people used the enum type in a switch statement, some compilers will warn if the
            // count enum value wasn't handled in the switch.
            // A define could be used instead. Reading: https://stackoverflow.com/questions/10157181/in-which-situations-anonymous-enum-should-be-used
            file.Append($"enum");
            file.StartCodeBlock();
            file.AppendLine($"{enumValueName} = {count}");
            file.FinishCodeBlock(";");
        }
    }
}