#nullable enable

using FluentAssertions;
using StateSmith.Output;
using StateSmith.Runner;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace StateSmithTest.Output.SmDescriberTest;

public class SmDesignDescriberTests
{
    // https://github.com/StateSmith/StateSmith/issues/452
    [Fact]
    public void IntegrationTestPlantuml_OutputDir()
    {
        var plantUmlText = $"""
            @startuml MySm
            [*] --> s1

            /'! $CONFIG : toml
                [SmRunnerSettings.smDesignDescriber]
                enabled = true
                outputDirectory = "meta-info/sub1/sub2"
                outputAncestorHandlers = true

                [SmRunnerSettings.smDesignDescriber.outputSections]
                beforeTransformations = true
                afterTransformations  = true
            '/
            @enduml
            """;

        var fakeFs = new CapturingCodeFileWriter();
        var relativeDir = TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter:fakeFs, transpilerId:TranspilerId.JavaScript);
        CapturingCodeFileWriter.Capture fileCapture = fakeFs.GetSoleCaptureWithName("MySm.md");

        fileCapture.filePath.Should().Be($"{relativeDir}/meta-info/sub1/sub2/MySm.md");
    }

    // actually writes to file so that we can get real console messages. If we were to use a fake file writer, we don't see printed messages.
    // https://github.com/StateSmith/StateSmith/issues/452
    [Fact]
    public void IntegrationTestConsoleMessage()
    {
        string smName = TestHelper.GenerateUniqueSmName();

        var plantUmlText = $"""
            @startuml {smName}
            [*] --> s1

            /'! $CONFIG : toml
                [SmRunnerSettings.smDesignDescriber]
                enabled = true
                outputDirectory = "meta-info"
                outputAncestorHandlers = true
            '/
            @enduml
            """;

        var console = new StringBuilderConsolePrinter();
        var relativeDir = TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, useRealFileWriter: true, consoleCapturer: console, transpilerId:TranspilerId.JavaScript);

        var printedConsole = console.sb.ToString();
        printedConsole.Should().Contain($"Writing to file `meta-info/{smName}.md`");

        // check that file actually exists
        File.Exists($"{relativeDir}/meta-info/{smName}.md").Should().BeTrue();
    }

    [Fact]
    public void IntegrationTestOffByDefault()
    {
        new Tester().Run().fakeFs.GetCapturesEndingWith(".md").Should().BeEmpty();
    }

    [Fact]
    public void IntegrationTestDisabled()
    {
        new Tester().Disable().Run().fakeFs.GetCapturesEndingWith(".md").Should().BeEmpty();
    }

    [Fact]
    public void IntegrationTestSmallEnabledDefault()
    {
        new Tester(diagramFile: "Ex565Tiny.drawio").Enable().RunAndExpect("""
            BEFORE TRANSFORMATIONS
            ===========================================================

            Vertex: ROOT
            -----------------------------------------
            - Type: StateMachine
            - Diagram Id: 5
            - Initial State: ROOT.\<InitialState>

            ### Behaviors
                do / { x++; }


            Vertex: ROOT.\<InitialState>
            -----------------------------------------
            - Parent: ROOT
            - Type: InitialState
            - Diagram Id: 7OOmFb1p9FNjOZE7ZJ_8-8

            ### Behaviors
                TransitionTo(S1)


            Vertex: S1
            -----------------------------------------
            - Parent: GROUP
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-140

            ### Behaviors
                PRESS / { a++; }

                [a > 10] / { blah(); } TransitionTo(S1)


            Vertex: S2
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: R4VN_IkJmzDVKIfME6CC-146

            ### Behaviors
                PRESS / { b++; }


            Vertex: GROUP
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: R4VN_IkJmzDVKIfME6CC-149

            ### Behaviors
                do / { b++; }

                PRESS [x > 10] TransitionTo(S2)

            """);
    }


    [Fact]
    public void IntegrationTestEnableAncestors()
    {
        new Tester(diagramFile: "Ex565Tiny.drawio").Enable().EnableOutputAncestorHandlers().RunAndExpect("""
            BEFORE TRANSFORMATIONS
            ===========================================================

            Vertex: ROOT
            -----------------------------------------
            - Type: StateMachine
            - Diagram Id: 5
            - Initial State: ROOT.\<InitialState>

            ### Behaviors
                do / { x++; }


            Vertex: ROOT.\<InitialState>
            -----------------------------------------
            - Parent: ROOT
            - Type: InitialState
            - Diagram Id: 7OOmFb1p9FNjOZE7ZJ_8-8

            ### Behaviors
                TransitionTo(S1)


            Vertex: S1
            -----------------------------------------
            - Parent: GROUP
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-140

            ### Behaviors
                PRESS / { a++; }

                [a > 10] / { blah(); } TransitionTo(S1)

                =========== from ancestor GROUP ===========

                do / { b++; }

                PRESS [x > 10] TransitionTo(S2)

                =========== from ancestor ROOT ===========

                do / { x++; }


            Vertex: S2
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: R4VN_IkJmzDVKIfME6CC-146

            ### Behaviors
                PRESS / { b++; }

                =========== from ancestor ROOT ===========

                do / { x++; }


            Vertex: GROUP
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: R4VN_IkJmzDVKIfME6CC-149

            ### Behaviors
                do / { b++; }

                PRESS [x > 10] TransitionTo(S2)

                =========== from ancestor ROOT ===========

                do / { x++; }

            """);
    }


    [Fact]
    public void IntegrationTestBeforeTransformationsOnly()
    {
        new Tester().Enable().DisableAfterTransformations().EnableOutputAncestorHandlers().RunAndExpect(ExpectedBeforeTransformations);
    }

    [Fact]
    public void IntegrationTestAfterTransformationsOnly()
    {
        new Tester().Enable().DisableBeforeTransformations().EnableAfterTransformations().EnableOutputAncestorHandlers().RunAndExpect(ExpectedAfterTransformations);
    }

    [Fact]
    public void IntegrationTestBoth()
    {
        new Tester().Enable().EnableBeforeTransformations().EnableAfterTransformations().EnableOutputAncestorHandlers().RunAndExpect(ExpectedFull);
    }

    [Fact]
    public void IntegrationTestDisableBoth()
    {
        Action a = () => new Tester().DisableAfterTransformations().DisableBeforeTransformations().RunAndExpect(ExpectedAfterTransformations);
        a.Should().Throw<Exception>(); //todo-low: better exception
    }

    /// <summary>
    /// 
    /// </summary>
    private class Tester
    {
        public SmRunner smRunner;
        public SmDesignDescriber describer;
        public CapturingCodeFileWriter fakeFs = new();

        public Tester(string diagramFile = "Ex564.drawio")
        {
            smRunner = SetupSmRunner(out var diServiceProvider, diagramFile);
            describer = diServiceProvider.GetInstanceOf<SmDesignDescriber>();
        }

        public Tester Enable()
        {
            smRunner.Settings.smDesignDescriber.enabled = true;
            return this;
        }

        public Tester EnableOutputAncestorHandlers()
        {
            smRunner.Settings.smDesignDescriber.outputAncestorHandlers = true;
            return this;
        }

        public Tester Disable()
        {
            smRunner.Settings.smDesignDescriber.enabled = false;
            return this;
        }

        public Tester DisableBeforeTransformations()
        {
            smRunner.Settings.smDesignDescriber.outputSections.beforeTransformations = false;
            return this;
        }

        public Tester DisableAfterTransformations()
        {
            smRunner.Settings.smDesignDescriber.outputSections.afterTransformations = false;
            return this;
        }

        public Tester EnableBeforeTransformations()
        {
            smRunner.Settings.smDesignDescriber.outputSections.beforeTransformations = true;
            return this;
        }

        public Tester EnableAfterTransformations()
        {
            smRunner.Settings.smDesignDescriber.outputSections.afterTransformations = true;
            return this;
        }

        public void RunAndExpect(string expected)
        {
            Run();
            describer.GetOutput().ShouldBeShowDiff(expected, outputCleanActual:true);
            fakeFs.GetCapturesEndingWith(".md").Single().code.ShouldBeShowDiff(expected, outputCleanActual:true);
        }

        public Tester Run()
        {
            smRunner.Run();
            return this;
        }

        private SmRunner SetupSmRunner(out DiServiceProvider di, string diagramFile)
        {
            SmRunner smRunner = new(diagramPath: TestHelper.GetThisDir() + diagramFile);
            smRunner.Settings.propagateExceptions = true; // for testing
            smRunner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<ICodeFileWriter>(fakeFs);
            smRunner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<IConsolePrinter>(new DiscardingConsolePrinter());

            di = smRunner.GetExperimentalAccess().DiServiceProvider;
            di.AddSingletonT<ICodeGenRunner>(new DummyCodeGenRunner()); // to make test run faster

            return smRunner;
        }
    }

    private const string ExpectedFull = $"""
            {ExpectedBeforeTransformations}


            <br>
            <br>
            <br>
            <br>
            <br>
            <br>

            {ExpectedAfterTransformations}
            """;


    private const string ExpectedBeforeTransformations = """
            BEFORE TRANSFORMATIONS
            ===========================================================

            Vertex: \<Notes>
            -----------------------------------------
            - Type: NotesVertex
            - Diagram Id: 140

            ### Notes Content:
                This is some stuff.
                
                Bold text.


            Vertex: \<RenderConfig>
            -----------------------------------------
            - Type: RenderConfigVertex
            - Diagram Id: 141


            Vertex: \<Config>(AutoExpandedVars)
            -----------------------------------------
            - Parent: \<RenderConfig>
            - Type: ConfigOptionVertex
            - Diagram Id: 145

            ### Option Content:
                int x;
                int y;


            Vertex: NORMAL
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: 138

            ### Behaviors
                enter / { x = 0; }

                do / { x++; }

                exit / { x = 15; }

                [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ROOT
            -----------------------------------------
            - Type: StateMachine
            - Diagram Id: 5
            - Initial State: ROOT.\<InitialState>

            ### Behaviors
                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ROOT.\<InitialState>
            -----------------------------------------
            - Parent: ROOT
            - Type: InitialState
            - Diagram Id: 8

            ### Behaviors
                TransitionTo(DELAY)


            Vertex: DELAY
            -----------------------------------------
            - Parent: NORMAL
            - Type: State
            - Diagram Id: 9

            ### Behaviors
                enter / { buzz(); }

                EV1 / { y = 33; }

                exit / { stop_buzz(); }

                [after_ms(100)
                && x >= 13] TransitionTo(RUNNING)

                =========== from ancestor NORMAL ===========

                do / { x++; }

                [err] TransitionTo(ERROR)
            
                =========== from ancestor ROOT ===========

                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ERROR
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-140

            ### Behaviors
                PRESS / { buzz(); }

                =========== from ancestor ROOT ===========

                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: RUNNING
            -----------------------------------------
            - Parent: NORMAL
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-143
            - Initial State: RUNNING.\<InitialState>

            ### Behaviors

                =========== from ancestor NORMAL ===========

                do / { x++; }

                [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: \<RenderConfig>
            -----------------------------------------
            - Parent: ROOT
            - Type: RenderConfigVertex
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-146


            Vertex: \<Config>(TriggerMap)
            -----------------------------------------
            - Parent: \<RenderConfig>
            - Type: ConfigOptionVertex
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-148

            ### Option Content:
                // some comment
                ANY => * /* wildcard */


            Vertex: PRE_HEAT
            -----------------------------------------
            - Parent: RUNNING
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-150

            ### Behaviors
                enter / { set_timeout(5 min);
                x *= 23; }

                =========== from ancestor NORMAL ===========

                do / { x++; }

                [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                ANY / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: RUNNING.\<InitialState>
            -----------------------------------------
            - Parent: RUNNING
            - Type: InitialState
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-152

            ### Behaviors
                TransitionTo(PRE_HEAT)

            """;

    private const string ExpectedAfterTransformations = """
            AFTER TRANSFORMATIONS
            ===========================================================

            Vertex: NORMAL
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: 138

            ### Behaviors
                enter / { x = 0; }

                do / { x++; }

                exit / { x = 15; }

                do [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ROOT
            -----------------------------------------
            - Type: StateMachine
            - Diagram Id: 5
            - Initial State: ROOT.\<InitialState>

            ### Behaviors
                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ROOT.\<InitialState>
            -----------------------------------------
            - Parent: ROOT
            - Type: InitialState
            - Diagram Id: 8

            ### Behaviors
                TransitionTo(DELAY)


            Vertex: DELAY
            -----------------------------------------
            - Parent: NORMAL
            - Type: State
            - Diagram Id: 9

            ### Behaviors
                enter / { buzz(); }

                EV1 / { y = 33; }

                exit / { stop_buzz(); }

                do [after_ms(100)
                && x >= 13] TransitionTo(RUNNING)

                =========== from ancestor NORMAL ===========

                do / { x++; }

                do [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: ERROR
            -----------------------------------------
            - Parent: ROOT
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-140

            ### Behaviors
                PRESS / { buzz(); }

                =========== from ancestor ROOT ===========

                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: RUNNING
            -----------------------------------------
            - Parent: NORMAL
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-143
            - Initial State: RUNNING.\<InitialState>

            ### Behaviors

                =========== from ancestor NORMAL ===========

                do / { x++; }

                do [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: PRE_HEAT
            -----------------------------------------
            - Parent: RUNNING
            - Type: State
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-150

            ### Behaviors
                enter / { set_timeout(5 min);
                x *= 23; }

                =========== from ancestor NORMAL ===========

                do / { x++; }

                do [err] TransitionTo(ERROR)

                =========== from ancestor ROOT ===========

                (INC_ERR, do, EV1, PRESS) / { log("unhandled event"); }

                INC_ERR / { err++; }


            Vertex: RUNNING.\<InitialState>
            -----------------------------------------
            - Parent: RUNNING
            - Type: InitialState
            - Diagram Id: gO1ZRfetmGtumTaL4_i4-152

            ### Behaviors
                TransitionTo(PRE_HEAT)
            
            """;

}
