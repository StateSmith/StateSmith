#nullable enable

using FluentAssertions;
using StateSmith.Input.Expansions;
using StateSmith.Output.Algos.Balanced1;
using StateSmith.Output.UserConfig;
using StateSmithTest.Output;
using StateSmithTest.Output.UserConfig;
using System.Linq;
using Xunit;

namespace StateSmithTest.Input.Expansions;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/284
/// </summary>
public class DefaultExpansionsTest_284
{
    [Fact]
    public void SmallerTest()
    {
        RenderConfigBaseVars renderConfig = new();
        Expander expander = new();
        DefaultExpansionsProcessor processor = new(renderConfig, expander, new CExpansionVarsPathProvider());

        renderConfig.DefaultVarExpTemplate = "{VarsPath}my_inputs.{AutoNameCopy()}";
        renderConfig.DefaultFuncExpTemplate = "{VarsPath}my_funcs.{AutoNameCopy()}";
        renderConfig.DefaultAnyExpTemplate = "this-should-be-ignored-as-others-are-set";

        processor.AddExpansions();
        expander.TryExpandVariableExpansion("""myLogger""").Should().Be("sm->vars.my_inputs.myLogger");
        expander.TryExpandFunctionExpansion("""log""", ["""s1 enter"""], """("s1 enter")""").Should().Be("""sm->vars.my_funcs.log("s1 enter")""");
    }

    class RenderConfig1 : IRenderConfig
    {
        public string DefaultVarExpTemplate => "{VarsPath}my_inputs.{AutoNameCopy()}";
        public string DefaultFuncExpTemplate => "{VarsPath}my_funcs.{AutoNameCopy()}";
        public string DefaultAnyExpTemplate => "this-should-be-ignored-as-others-are-set";
    }

    [Fact]
    public void IntegrationTest()
    {
        var plantUmlText = """
            @startuml ExampleSm
            [*] --> s1: / log("init to s1");
            s1: enter / myLogger.log("s1 enter");
            @enduml
            """;

        var renderConfig = new RenderConfigFields()
        {
            DefaultVarExpTemplate = "{VarsPath}my_inputs.{AutoNameCopy()}",
            DefaultFuncExpTemplate = "{VarsPath}my_funcs.{AutoNameCopy()}",
            DefaultAnyExpTemplate = "this-should-be-ignored-as-others-are-set"
        };

        var fakeFs = new CapturingCodeFileWriter();
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, renderConfig: renderConfig, codeFileWriter: fakeFs);
        var code = fakeFs.GetCapturesForFileName("ExampleSm.c").Single().code;

        code.Should().Contain("""sm->vars.my_funcs.log("init to s1");""");
        code.Should().Contain("""sm->vars.my_inputs.myLogger.log("s1 enter");""");
    }

    [Fact]
    public void IntegrationTestAny()
    {
        var plantUmlText = """
            @startuml ExampleSm
            [*] --> s1: / log("init to s1");
            s1: enter / myLogger.log("s1 enter");
            @enduml
            """;

        var renderConfig = new RenderConfigFields()
        {
            DefaultAnyExpTemplate = "my_stuff.{AutoNameCopy()}"
        };

        var fakeFs = new CapturingCodeFileWriter();
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, renderConfig: renderConfig, codeFileWriter: fakeFs);
        var code = fakeFs.GetCapturesForFileName("ExampleSm.c").Single().code;

        code.Should().Contain("""my_stuff.log("init to s1");""");
        code.Should().Contain("""my_stuff.myLogger.log("s1 enter");""");
    }

    [Fact]
    public void IntegrationTestToml()
    {
        var plantUmlText = """
            @startuml ExampleSm
            [*] --> s1: / log("init to s1");
            s1: enter / myLogger.log("s1 enter");

            /'! $CONFIG : toml
                [RenderConfig]
                DefaultVarExpTemplate = "{VarsPath}my_inputs.{AutoNameCopy()}"
                DefaultFuncExpTemplate = "{VarsPath}my_funcs.{AutoNameCopy()}"
                DefaultAnyExpTemplate = "this-should-be-ignored-as-others-are-set"
            '/
            @enduml
            """;

        var fakeFs = new CapturingCodeFileWriter();
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter: fakeFs);

        var code = fakeFs.GetCapturesForFileName("ExampleSm.c").Single().code;

        code.Should().Contain("""sm->vars.my_funcs.log("init to s1");""");
    }
}
