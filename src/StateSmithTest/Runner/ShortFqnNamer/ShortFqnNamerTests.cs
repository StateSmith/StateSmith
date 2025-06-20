#nullable enable

using FluentAssertions;
using StateSmith.Runner;
using StateSmith.SmGraph;
using StateSmith.SmGraph.Validation;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace StateSmithTest.Runner.ShortFqnNamer;

/// <summary>
/// NOTE! this class uses diagrams that aren't state machines, but are used to test the ShortFqnNamer.
/// </summary>
public class ShortFqnNamerTests
{
    bool resolveWithHighestState = true;

    /// <summary>
    /// We need to disable the pre diagram based settings parsing because these files are not state machines.
    /// </summary>
    const bool enablePDBS = false;

    [Fact]
    public void SimpleTest()
    {
        Run("Simple.drawio", Test);

        static void Test(StateMachine sm)
        {
            NamedVertexMap map = new(sm);

            // manually check some states
            map.GetState("A");
            map.GetState("A__S1");
            map.GetState("A__S2");
            map.GetState("A__G1");
            map.GetState("A__G2");
            //
            map.GetState("B");
            map.GetState("B__S1");
            map.GetState("B__S2");
            map.GetState("B__G1");
            map.GetState("B__G2");

            sm.VisitTypeRecursively<NamedVertex>(skipSelf: true, vertex =>
            {
                vertex.Behaviors.Single().Triggers.Single().Should().Be(vertex.Name);
            });
        }
    }

    [Fact]
    public void Bad1Test()
    {
        Action a = () => Run("Bad1.drawio");
        a.Should().Throw<VertexValidationException>().WithMessage("*multiple children with the same name `G1`*");
    }

    [Fact]
    public void MenuHigher()
    {
        Run("MenuHigher.drawio", Test);

        static void Test(StateMachine sm)
        {
            NamedVertexMap map = new(sm);
            map.GetState("DRINK__NONE");
            map.GetState("FOOD_BEV__NONE__NONE");

            sm.VisitTypeRecursively<NamedVertex>(skipSelf: true, vertex =>
            {
                vertex.Behaviors.Single().Triggers.Single().Should().Be(vertex.Name);
            });
        }
    }

    [Fact]
    public void MenuHigherFromSettings()
    {
        RunWithSettings(Test, RunnerSettings.NameConflictResolution.ShortFqnAncestor, "MenuHigher.drawio");

        static void Test(StateMachine sm)
        {
            NamedVertexMap map = new(sm);
            map.GetState("DRINK__NONE");
            map.GetState("FOOD_BEV__NONE__NONE");

            sm.VisitTypeRecursively<NamedVertex>(skipSelf: true, vertex =>
            {
                vertex.Behaviors.Single().Triggers.Single().Should().Be(vertex.Name);
            });
        }
    }

    [Fact]
    public void MenuLower()
    {
        resolveWithHighestState = false;
        Run("MenuLower.drawio", Test);

        static void Test(StateMachine sm)
        {
            NamedVertexMap map = new(sm);
            map.GetState("DRINK__COLD__NONE");
            map.GetState("FOOD_BEV__NONE__NONE");

            sm.VisitTypeRecursively<NamedVertex>(skipSelf: true, vertex =>
            {
                vertex.Behaviors.Single().Triggers.Single().Should().Be(vertex.Name);
            });
        }
    }

    [Fact]
    public void MenuLowerFromSettings()
    {
        RunWithSettings(Test, RunnerSettings.NameConflictResolution.ShortFqnParent, "MenuLower.drawio");

        static void Test(StateMachine sm)
        {
            NamedVertexMap map = new(sm);
            map.GetState("DRINK__COLD__NONE");
            map.GetState("FOOD_BEV__NONE__NONE");

            sm.VisitTypeRecursively<NamedVertex>(skipSelf: true, vertex =>
            {
                vertex.Behaviors.Single().Triggers.Single().Should().Be(vertex.Name);
            });
        }
    }

    private static void RunWithSettings(Action<StateMachine> testMethod, RunnerSettings.NameConflictResolution resolutionSetting, string DiagramPath)
    {
        SmRunner runner = new(diagramPath: DiagramPath, enablePDBS: enablePDBS);
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_NameConflictResolution, (TransformationStep)HierachicalGraphToSmConverter.Convert);
        runner.SmTransformer.InsertAfterFirstMatch(StandardSmTransformer.TransformationId.Standard_FinalValidation, (TransformationStep)testMethod);

        runner.Settings.propagateExceptions = true; // for unit testing
        runner.Settings.outputDirectory = Path.GetTempPath(); // for unit testing
        runner.Settings.nameConflictResolution = resolutionSetting;
        runner.Run();
    }

    private void Rename(StateMachine sm)
    {
        var namer = new StateSmith.Runner.ShortFqnNamer(resolveWithAncestor: resolveWithHighestState);
        namer.ResolveNameConflicts(sm);
    }

    private void Run(string diagramPath, Action<StateMachine>? testMethod = null)
    {
        SmRunner runner = new(diagramPath: diagramPath, enablePDBS: enablePDBS);
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_Validation1, (TransformationStep)HierachicalGraphToSmConverter.Convert);
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_Validation1, (TransformationStep)Rename);

        if (testMethod != null)
            runner.SmTransformer.InsertAfterFirstMatch(StandardSmTransformer.TransformationId.Standard_FinalValidation, (TransformationStep)testMethod);

        runner.Settings.propagateExceptions = true; // for unit testing
        runner.Settings.outputDirectory = Path.GetTempPath(); // for unit testing
        runner.Settings.nameConflictResolution = RunnerSettings.NameConflictResolution.Manual;
        runner.Run();
    }

    [Fact]
    public void TestGraphConverter()
    {
        SmRunner runner = new(diagramPath: "HierachicalGraphConverterEx1.drawio", enablePDBS: enablePDBS);
        runner.SmTransformer.InsertBeforeFirstMatch(StandardSmTransformer.TransformationId.Standard_Validation1, (TransformationStep)HierachicalGraphToSmConverter.Convert);
        runner.SmTransformer.InsertAfterFirstMatch(StandardSmTransformer.TransformationId.Standard_FinalValidation, (TransformationStep)Test);
        runner.Settings.propagateExceptions = true; // for unit testing
        runner.Settings.outputDirectory = Path.GetTempPath(); // for unit testing
        runner.Run();

        static void Test(StateMachine sm)
        {
            NamedVertexMap map = new(sm);
            map.GetState("A").Parent.Should().Be(sm);
            map.GetState("A").ShouldHaveChildrenAndUmlBehaviors("S1", "A");
            map.GetState("S1").ShouldHaveChildrenAndUmlBehaviors("S2", "A__S1");
            map.GetState("S2").ShouldHaveChildrenAndUmlBehaviors("G1, G2", "A__S2");
            map.GetState("G1").ShouldHaveChildrenAndUmlBehaviors("", "A__G1");
            map.GetState("G2").ShouldHaveChildrenAndUmlBehaviors("", "A__G2");
        }
    }

    [Fact]
    public void NestedClash1()
    {
        string relativeFilePath = "NestedClash1.drawio";
        TestNestedClashX(relativeFilePath);
    }

    [Fact]
    public void NestedClash2()
    {
        string relativeFilePath = "NestedClash2.drawio";
        TestNestedClashX(relativeFilePath);
    }

    private static void TestNestedClashX(string relativeFilePath)
    {
        InputSmBuilder inputSmBuilder = TestHelper.CreateInputSmBuilder();
        inputSmBuilder.ConvertDrawIoFileNodesToVertices(TestHelper.GetThisDir() + relativeFilePath);
        inputSmBuilder.FinishRunning();

        NamedVertexMap map = new(inputSmBuilder.GetStateMachine());
        map.GetState("S1").ShouldHaveUmlBehaviors("EV1");
        map.GetState("S1__S1").ShouldHaveUmlBehaviors("EV2");
        map.GetState("S1__S1__S1").ShouldHaveUmlBehaviors("EV3");
        map.GetState("S1__S2__S1").ShouldHaveUmlBehaviors("EV4");

        map.GetState("S1__S1__S2").ShouldHaveUmlBehaviors("EV20");
        map.GetState("S1__S2").ShouldHaveUmlBehaviors("EV21");
    }
}
