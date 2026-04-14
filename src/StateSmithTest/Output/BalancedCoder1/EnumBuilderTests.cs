using StateSmith.Output;
using StateSmith.Output.Algos.Balanced1;
using StateSmith.Runner;
using StateSmith.SmGraph;
using Xunit;

namespace StateSmithTest.Output.BalancedCoder1;

public class EnumBuilderTests
{
    [Fact]
    public void Test1()
    {
        StateMachine sm = new("MySm1");
        sm.AddChild(new State("S1"));
        sm.AddChild(new State("S2"));

        sm._events.Add("EV1");
        sm._events.Add("EV2");

        OutputFile file = new(new CodeStyleSettings(), new());
        EnumBuilder enumBuilder = new(new NameMangler(sm), new StateMachineProvider(sm), new());

        enumBuilder.OutputEventIdCode(file);
        file.AppendIndentedLine();
        enumBuilder.OutputStateIdCode(file);

        file.ToString().ShouldBeShowDiff("""
            public enum EventId
            {
                EV1 = 0,
                EV2 = 1,
            }

            public const int EventIdCount = 2;

            public enum StateId
            {
                ROOT = 0,
                S1 = 1,
                S2 = 2,
            }

            public const int StateIdCount = 3;
            
            // Subtree meta data generation can be disabled in settings.
            // Details: https://github.com/StateSmith/StateSmith/issues/538
            public const int ROOT_SubtreeEndId = 2;  // State 'MySm1' subtree extends from itself (id: 0) to state 'S2' (id: 2)
            public const int S1_SubtreeEndId = 1;  // State 'S1' subtree extends from itself (id: 1) to state 'S1' (id: 1)
            public const int S2_SubtreeEndId = 2;  // State 'S2' subtree extends from itself (id: 2) to state 'S2' (id: 2)
            
            """);
    }
}
