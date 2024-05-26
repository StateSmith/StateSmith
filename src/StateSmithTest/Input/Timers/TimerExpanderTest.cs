using FluentAssertions;
using StateSmith.Input.Timers;
using StateSmith.Output;
using Xunit;

namespace StateSmithTest.Input.Timers;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/76
/// </summary>
public class TimerExpanderTest_76
{
    TimerExpander expander = new(new TestExpVarsPathProvider(), new TimerExpanderSettings() { timeAccessCode = "millis()" });
    private const string TIME_IN_STATE_VAR_NAME = "_time_in_state";


    /// <summary>
    /// No change to input string.
    /// </summary>
    [Fact]
    public void PassThroughTests()
    {
        expander.TryExpandVariableExpansion("this.vars.foo").Should().Be("this.vars.foo");
    }

    [Fact]
    public void TestTimeInState()
    {
        expander.TryExpandVariableExpansion("$tis").Should().Be(TIME_IN_STATE_VAR_NAME);
        expander.TryExpandVariableExpansion("$time_in_state").Should().Be(TIME_IN_STATE_VAR_NAME);
    }

    [Fact]
    public void TestStateEnteredTime()
    {
        expander.TryExpandVariableExpansion("$state_entered_time").Should().Be("this.vars._timer_level_0_start");
    }

    [Fact]
    public void TestTime()
    {
        expander.TryExpandVariableExpansion("$time").Should().Be("millis()");
    }

    public class TestExpVarsPathProvider : IExpansionVarsPathProvider
    {
        public string ExpansionVarsPath => "this.vars.";
    }
}
