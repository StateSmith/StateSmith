using StateSmith.Output.Gil;
using StateSmith.SmGraph;
using StateSmithTest.spec2;
using System.Linq;
using Xunit;

namespace StateSmithTest.PseudoStateTests;

public class HistoryContinueValidationTests : ValidationTestHelper
{
    private StateMachine root;
    private HistoryContinueVertex hc1;
    private HistoryContinueVertex hc2;
    private HistoryVertex h1;
    private string h1VarName = Test7History1Helper.SmVars + ".G1_history";
    private string h1EnumAccess = "G1_HistoryId.";
    NamedVertexMap map;
    State GetState(string stateName) => map.GetState(stateName);

    public const string GilStart = GilCreationHelper.GilExpansionMarkerFuncName + "(";
    public const string GilEnd = ")";

    public HistoryContinueValidationTests()
    {
        var plantUmlText = """
            @startuml ExampleSm
            [*] --> G1
            state G1 {
                [*] --> [H]
                [H] --> G2
                state G2 {
                    [*] --> G3
                    state "$HC" as hc1
                    state G3 {
                        state "$HC" as hc2
                        [*] --> G4
                    }
                }
            }
            @enduml
            """;
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices(plantUmlText);
        inputSmBuilder.SetupForSingleSm();        
        root = inputSmBuilder.GetStateMachine();
        map = new NamedVertexMap(inputSmBuilder.GetStateMachine());
        hc1 = GetState("G2").ChildType<HistoryContinueVertex>();
        hc2 = GetState("G3").ChildType<HistoryContinueVertex>();
        h1 = GetState("G1").ChildType<HistoryVertex>();
    }

    [Fact]
    public void Ok_DefaultNoActionCode()
    {
        RunCompiler();

        h1.ShouldHaveUmlBehaviors($"""
            [{GilStart}{h1VarName} == {h1EnumAccess}G3{GilEnd}] TransitionTo(G3)
            [{GilStart}{h1VarName} == {h1EnumAccess}G4{GilEnd}] TransitionTo(G4)
            else TransitionTo(G2)
            """
        );

        GetState("G2").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G2;{{GilEnd}} }""");
        GetState("G3").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G3;{{GilEnd}} }""");
        GetState("G4").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G4;{{GilEnd}} }""");
    }

    [Fact]
    public void Ok_RemoveUnreachableStates()
    {
        // remove incoming transition to G4. Should see it no longer tracked.
        GetState("G3").ChildType<InitialState>().RemoveSelf();
        RunCompiler();

        h1.ShouldHaveUmlBehaviors($"""
            [{GilStart}{h1VarName} == {h1EnumAccess}G3{GilEnd}] TransitionTo(G3)
            else TransitionTo(G2)
            """
        );

        GetState("G2").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G2;{{GilEnd}} }""");
        GetState("G3").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G3;{{GilEnd}} }""");
        GetState("G4").ShouldHaveUmlBehaviors(""); // note that G4 has no incoming tarnsitions anymore. It is no longer tracked.
    }

    [Fact]
    public void Ok_RemoveUnnecessaryGroups()
    {
        // remove incoming transition to G3. Should see it no longer tracked.
        GetState("G3").IncomingTransitions.Single().RetargetTo(GetState("G4"));
        RunCompiler();

        h1.ShouldHaveUmlBehaviors($"""
            [{GilStart}{h1VarName} == {h1EnumAccess}G4{GilEnd}] TransitionTo(G4)
            else TransitionTo(G2)
            """
        );

        GetState("G2").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G2;{{GilEnd}} }""");
        GetState("G4").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G4;{{GilEnd}} }""");
        GetState("G3").ShouldHaveUmlBehaviors(""); // note that G3 has no incoming tarnsitions anymore. It is no longer tracked.
    }

    [Fact]
    public void Ok_DefaultWithActionCode()
    {
        // in this test, the default transition has action code which prevents it from being re-used.
        h1.Behaviors.Single().actionCode = "p = 100;";
        RunCompiler();

        h1.ShouldHaveUmlBehaviors($$"""
            [{{GilStart}}{{h1VarName}} == {{h1EnumAccess}}G2{{GilEnd}}] TransitionTo(G2)
            [{{GilStart}}{{h1VarName}} == {{h1EnumAccess}}G3{{GilEnd}}] TransitionTo(G3)
            [{{GilStart}}{{h1VarName}} == {{h1EnumAccess}}G4{{GilEnd}}] TransitionTo(G4)
            else / { p = 100; } TransitionTo(G2)
            """
        );

        GetState("G2").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G2;{{GilEnd}} }""");
        GetState("G3").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G3;{{GilEnd}} }""");
        GetState("G4").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G4;{{GilEnd}} }""");
    }

    [Fact]
    public void Ok_DefaultToPseudoState()
    {
        // in this test, the default transition goes to a pseudo state
        var c1 = h1.Parent.AddChild(new ChoicePoint());
        c1.AddBehavior(new Behavior(transitionTarget: h1.Behaviors.Single().TransitionTarget));
        h1.Behaviors.Single().RetargetTo(c1);
        RunCompiler();

        h1.ShouldHaveUmlBehaviors($$"""
            [{{GilStart}}{{h1VarName}} == {{h1EnumAccess}}G2{{GilEnd}}] TransitionTo(G2)
            [{{GilStart}}{{h1VarName}} == {{h1EnumAccess}}G3{{GilEnd}}] TransitionTo(G3)
            [{{GilStart}}{{h1VarName}} == {{h1EnumAccess}}G4{{GilEnd}}] TransitionTo(G4)
            else TransitionTo(G1.<ChoicePoint>())
            """
        );

        GetState("G2").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G2;{{GilEnd}} }""");
        GetState("G3").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G3;{{GilEnd}} }""");
        GetState("G4").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G4;{{GilEnd}} }""");
    }

    [Fact]
    public void GuardOnDefaultTransition()
    {
        h1.Behaviors.Single().guardCode = "p > 100";
        ExpectVertexValidationExceptionWildcard("*default transition*");
    }

    [Fact]
    public void Duplicate()
    {
        GetState("G2").AddChild(new HistoryContinueVertex());
        ExpectVertexValidationExceptionWildcard("* 1 HistoryContinue* allowed*. Found *2*");
    }

    [Fact]
    public void NoBehaviorsAllowed()
    {
        hc1.AddBehavior(new Behavior(actionCode: "x++;"));
        ExpectVertexValidationExceptionWildcard("* HistoryContinue* cannot have any behaviors*. Found *1*");
    }

    [Fact]
    public void NoKidsAllowed()
    {
        hc1.AddChild(new State("blahblah"));
        ExpectVertexValidationException(exceptionMessagePart: "children");
    }

    [Fact]
    public void ParentNonNull()
    {
        hc1._parent = null;
        ExpectVertexValidationException(exceptionMessagePart: "parent");
    }

    [Fact]
    public void GapException1()
    {
        hc1.RemoveSelf();
        ExpectVertexValidationException(exceptionMessagePart: "two levels up");
    }

    [Fact]
    public void HistoryInsteadOfHc1Above()
    {
        // h2 is in G2
        var h2 = hc1.Parent.AddChild(new HistoryVertex());  h2.AddTransitionTo(GetState("G3"));
        var h2VarName = Test7History1Helper.SmVars + ".G2_history";
        var h2EnumAccess = "G2_HistoryId.";
        hc1.RemoveSelf();
        RunCompiler();

        h1.ShouldHaveUmlBehaviors($$"""
            else TransitionTo(G2)
            """
        );

        h2.ShouldHaveUmlBehaviors($$"""
            [{{GilStart}}{{h2VarName}} == {{h2EnumAccess}}G4{{GilEnd}}] TransitionTo(G4)
            else TransitionTo(G3)
            """
        );

        GetState("G2").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G2;{{GilEnd}} }""");
        GetState("G3").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h2VarName}} = {{h2EnumAccess}}G3;{{GilEnd}} }""");
        GetState("G4").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h2VarName}} = {{h2EnumAccess}}G4;{{GilEnd}} }""");
    }

    [Fact]
    public void AdditionalHistoryBesideHc1()
    {
        // h2 is in G2
        var h2 = hc1.Parent.AddChild(new HistoryVertex()); h2.AddTransitionTo(GetState("G3"));
        var h2VarName = Test7History1Helper.SmVars + ".G2_history";
        var h2EnumAccess = "G2_HistoryId.";
        RunCompiler();

        h1.ShouldHaveUmlBehaviors($"""
            [{GilStart}{h1VarName} == {h1EnumAccess}G3{GilEnd}] TransitionTo(G3)
            [{GilStart}{h1VarName} == {h1EnumAccess}G4{GilEnd}] TransitionTo(G4)
            else TransitionTo(G2)
            """
        );

        GetState("G2").ShouldHaveUmlBehaviors($$"""enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G2;{{GilEnd}} }""");
        GetState("G3").ShouldHaveUmlBehaviors($$"""
            enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G3;{{GilEnd}} }
            enter / { {{GilStart}}{{h2VarName}} = {{h2EnumAccess}}G3;{{GilEnd}} }
            """);
        GetState("G4").ShouldHaveUmlBehaviors($$"""
            enter / { {{GilStart}}{{h1VarName}} = {{h1EnumAccess}}G4;{{GilEnd}} }
            enter / { {{GilStart}}{{h2VarName}} = {{h2EnumAccess}}G4;{{GilEnd}} }
            """);
    }
}
