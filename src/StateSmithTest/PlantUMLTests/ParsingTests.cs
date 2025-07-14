#nullable enable

using System;
using System.Collections.Generic;
using Xunit;
using StateSmith.Input.PlantUML;
using StateSmith.SmGraph;
using StateSmith.Input;
using FluentAssertions;
using StateSmith.Runner;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace StateSmithTest.PlantUMLTests;

public class ParsingTests
{
    private PlantUMLToNodesEdges translator = new();

    [Fact]
    public void DiagramName()
    {
        ParseAssertNoError("""
            @startuml MyPumlSm1

            @enduml
            """);
        translator.Root.id.Should().Be("MyPumlSm1");
    }

    [Fact]
    public void NoDiagramName()
    {
        ParseAssertNoError("""
            @startuml

            @enduml
            """);
        translator.Root.id.Should().Be("foo");
    }

    [Fact]
    public void InvalidDiagramName()
    {
        ParseAssertNoError("%$foo##$*.puml", """
            @startuml

            @enduml
            """);
        translator.Root.id.Should().Be("foo");
    }

    [Fact]
    public void InvalidDiagramNameStartsWithNumber()
    {
        ParseAssertNoError("22# $%foo##$*.puml", """
            @startuml

            @enduml
            """);
        translator.Root.id.Should().Be("sm22foo");
    }

    [Fact]
    public void DiagramNameIndented()
    {
        ParseAssertNoError("""
                @startuml MyPumlSm1

                @enduml
            """);
        translator.Root.id.Should().Be("MyPumlSm1");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/352
    /// </summary>
    [Fact]
    public void LineCommentsBeforeDiagramName_352()
    {
        ParseAssertNoError("""
            ' Single line comments (like this) start with a single quote.
            ' The line below specifies the name of the state machine.
            @startuml MyPumlSm1

            @enduml
            """);
        translator.Root.id.Should().Be("MyPumlSm1");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/352
    /// </summary>
    [Fact]
    public void BlockCommentsBeforeDiagramName_352()
    {
        ParseAssertNoError("""
            /' The line below specifies the name of the state machine. '/
            @startuml MyPumlSm1

            @enduml
            """);
        translator.Root.id.Should().Be("MyPumlSm1");
    }

    [Fact]
    public void InvalidInput()
    {
        ParseAssertHasAtLeastOneError("""
            @startuml MyPumlSm1 Blah

            @enduml
            """);
    }

    [Fact]
    public void NoDiagramNameThrows()
    {
        Action action = () => ParseAssertNoError("""
            @startuml
            State1 --> State2
            @enduml
            """);

    }

    [Fact]
    public void ThrowOnEndState()
    {
        Action action = () => ParseAssertNoError("""
            @startuml SomeName
            State1 --> [*]
            @enduml
            """);

        action.Should().Throw<Exception>()
            .WithMessage("StateSmith doesn't support end states. Location Details { line: 2, column: 0, text: `State1 --> [*]`. }");
    }

    [Fact]
    public void TwoStates()
    {
        ParseAssertNoError("""
            @startuml SomeSmName

            [*] --> State1
            State1 : enter / some_action();
            State1 : EVENT [guard] { action(); cout << Obj::cpp_rules(); x->v = 100 >> 2; }
            State1 -> State2 : EVENT2 [guard2] / tx_action();

            @enduml
            """);
        translator.Root.children.Count.Should().Be(3);

        DiagramNode initialState = translator.Root.children[0];
        DiagramNode state1 = translator.Root.children[1];
        DiagramNode state2 = translator.Root.children[2];

        initialState.label.Should().Be(VertexParseStrings.InitialStateString);
        state1.label.Should().Be("""
            State1
            enter / some_action();
            EVENT [guard] { action(); cout << Obj::cpp_rules(); x->v = 100 >> 2; }
            """);
        state2.label.Should().Be("State2");

        translator.Edges.Count.Should().Be(2);
        translator.Edges[0].source.Should().Be(initialState);
        translator.Edges[0].target.Should().Be(state1);
        //
        translator.Edges[1].source.Should().Be(state1);
        translator.Edges[1].target.Should().Be(state2);
        translator.Edges[1].label.Should().Be("EVENT2 [guard2] / tx_action();");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/369
    /// </summary>
    [Fact]
    public void NewLinesInDiagram_369()
    {
        ParseAssertNoError("""
            @startuml SomeSmName
            [*] --> State1: / printf("Count: %i\\n", count);
            @enduml
            """);
        translator.Edges.Count.Should().Be(1);
        translator.Edges[0].label.Should().Be("""/ printf("Count: %i\n", count);""");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/369
    /// https://github.com/StateSmith/StateSmith/issues/362
    /// </summary>
    [Fact]
    public void PlantUmlDecode_369_362()
    {
        // trailing 'a' is there to avoid having a line continuation sequence
        PlantUMLWalker.Decode("""\n a""").Should().Be("\n a");
        PlantUMLWalker.Decode("""\t a""").Should().Be("\t a");
        PlantUMLWalker.Decode("""\\n a""").Should().Be("""\n a""");
        PlantUMLWalker.Decode("""\\t a""").Should().Be("""\t a""");
        PlantUMLWalker.Decode("""\ a""").Should().Be("""\ a""");
        PlantUMLWalker.Decode("""\\ a""").Should().Be("""\ a""");
        PlantUMLWalker.Decode("""\\\ a""").Should().Be("""\\ a""");
        PlantUMLWalker.Decode("""\\\\ a""").Should().Be("""\\ a""");

        // non escape characters are just passed through
        PlantUMLWalker.Decode("""\a""").Should().Be("""\a""");

        // left/right alignment chars are removed
        // https://github.com/StateSmith/StateSmith/issues/362
        PlantUMLWalker.Decode("""\lblah(\l);\l""").Should().Be("""blah();""");
        PlantUMLWalker.Decode("""\rblah(\r);\r""").Should().Be("""blah();""");

        // if ends with \ then it is a line continuation which we don't support yet so we just ignore it
        // https://github.com/StateSmith/StateSmith/issues/379
        PlantUMLWalker.Decode("""\""").Should().Be("");
    }

    private void AssertNodeHasNoKids(DiagramNode node)
    {
        node.children.Should().BeEquivalentTo(new List<DiagramNode> {  });
    }

    [Fact]
    public void CompositeStatesWithLongName()
    {
        // mod from https://plantuml.com/state-diagram#3b0649c72650c313
        ParseAssertNoError("""
            @startuml DiagramName
            state A {
              state X {
              }
              state Y {
                state "Y_1" as State1 {
                    State1 : exit / some_exit_action(); x++; y--;
                }
              }
            }
             
            state B {
              state Z {
              }
            }

            X --> Z
            Z --> Y
            @enduml
            """);
        DiagramNode root = translator.Root;
        DiagramNode stateA = root.children[0];
        DiagramNode stateB = root.children[1];

        DiagramNode stateX = stateA.children[0];
        DiagramNode stateY = stateA.children[1];

        DiagramNode state1 = stateY.children[0];

        DiagramNode stateZ = stateB.children[0];

        root.children.Should().BeEquivalentTo(new List<DiagramNode> { stateA, stateB });
        stateA.children.Should().BeEquivalentTo(new List<DiagramNode>{ stateX, stateY });
        stateB.children.Should().BeEquivalentTo(new List<DiagramNode>{ stateZ });
        stateY.children.Should().BeEquivalentTo(new List<DiagramNode>{ state1 });
        AssertNodeHasNoKids(stateX);
        AssertNodeHasNoKids(stateZ);

        state1.label.Should().Be("""
            Y_1
            exit / some_exit_action(); x++; y--;
            """);

        translator.Edges.Count.Should().Be(2);
        translator.Edges[0].source.Should().Be(stateX);
        translator.Edges[0].target.Should().Be(stateZ);
        //
        translator.Edges[1].source.Should().Be(stateZ);
        translator.Edges[1].target.Should().Be(stateY);
    }

    private void ParseAssertNoError(string input)
    {
        ParseAssertNoError("foo.puml", input);
    }

    private void ParseAssertNoError(string filename, string input)
    {
        translator.ParseDiagramText(filename, input);
        translator.HasError().Should().BeFalse();
    }

    private void ParseAssertHasAtLeastOneError(string input)
    {
        translator.ParseDiagramText("foo.puml", input);
        translator.HasError().Should().BeTrue();
    }


    /// <summary>
    /// See https://github.com/StateSmith/StateSmith/issues/3
    /// </summary>
    [Fact]
    public void EntryExitStates()
    {
        // input modified from https://plantuml.com/state-diagram#3b0649c72650c313
        ParseAssertNoError("""
            @startuml SompySm
            state Somp {
              state entry1 <<entryPoint>>
              state entry2 <<entrypoint>>    /' case insensitive allowed https://github.com/StateSmith/StateSmith/issues/227 '/
              state sin
              entry1 --> sin
              entry2 -> sin
              sin -> sin2
              sin2 --> exitA <<exitPoint>>
            }

            [*] --> entry1 : / initial_tx_action();
            exitA --> Foo
            Foo1 -> entry2 : EV1 [guard()] / action_e2();
            @enduml
            """);

        DiagramNode root = translator.Root;

        DiagramNode initial = root.children[1];
        DiagramNode Foo = GetNodeById("Foo");
        DiagramNode Foo1 = GetNodeById("Foo1");

        DiagramNode Somp = GetNodeById("Somp");
        DiagramNode entry1 = GetNodeById("entry1");
        DiagramNode entry2 = GetNodeById("entry2");
        DiagramNode exitA = GetNodeById("exitA");
        DiagramNode sin = GetNodeById("sin");
        DiagramNode sin2 = GetNodeById("sin2");

        entry1.label.Should().Be("entry : entry1");
        entry2.label.Should().Be("entry : entry2");
        exitA.label.Should().Be("exit : exitA");

        int i = 0;
        AssertEdge(translator.Edges[i++], source: entry1, target: sin, label: "");
        AssertEdge(translator.Edges[i++], source: entry2, target: sin, label: "");
        AssertEdge(translator.Edges[i++], source: sin, target: sin2, label: "");
        AssertEdge(translator.Edges[i++], source: sin2, target: exitA, label: "");
        // following edges need re-routing and label adjustments
        AssertEdge(translator.Edges[i++], source: initial, target: /* was entry1 */ Somp, label: "/ initial_tx_action(); via entry entry1");
        AssertEdge(translator.Edges[i++], source: /* was exitA */ Somp, target: Foo, label: "via exit exitA");
        AssertEdge(translator.Edges[i++], source: Foo1, target: /* was entry2 */ Somp, label: "EV1 [guard()] / action_e2(); via entry entry2");

        // ensure entry and exit validation works

        IServiceProvider serviceProvider = TestHelper.CreateServiceProvider();
        InputSmBuilder inputSmBuilder = serviceProvider.GetRequiredService<InputSmBuilder>();
        inputSmBuilder.ConvertNodesToVertices(new List<DiagramNode> { translator.Root }, translator.Edges);
        inputSmBuilder.FinishRunning();
    }


    /// <summary>
    /// See https://github.com/StateSmith/StateSmith/issues/40
    /// </summary>
    [Fact]
    public void ChoicePoints()
    {
        var plantUmlText = """
            @startuml ExampleSm
            state c1 <<choice>>
            [*] --> c1
            c1 --> s1 : [id <= 10]
            c1 --> s2 : else
            @enduml
            """;
        IServiceProvider serviceProvider = TestHelper.CreateServiceProvider();
        InputSmBuilder inputSmBuilder = serviceProvider.GetRequiredService<InputSmBuilder>();
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices("foo.puml", plantUmlText);
        inputSmBuilder.FinishRunning();

        StateMachine root = inputSmBuilder.GetStateMachine();
        InitialState initial = root.ChildType<InitialState>();
        ChoicePoint c1 = root.ChildType<ChoicePoint>();
        State s1 = root.Child<State>("s1");
        State s2 = root.Child<State>("s2");

        c1.label.Should().Be("c1");

        var behaviorMatcher = FABehaviorMatcherBuilder.Build(actionCode: true, guardCode: true, transitionTarget: true, triggers: true);

        initial.Behaviors.Should().BeEquivalentTo(new List<Behavior>() {
            new Behavior(){ _transitionTarget = c1 },
        }, behaviorMatcher);

        c1.Behaviors.Should().BeEquivalentTo(new List<Behavior>() {
            new Behavior(){ _transitionTarget = s1, guardCode = "id <= 10" },
            new Behavior(){ _transitionTarget = s2 },
        }, behaviorMatcher);
    }

    /// <summary>
    /// See https://github.com/StateSmith/StateSmith/issues/227
    /// </summary>
    [Fact]
    public void ChoicePointsCaseInsensitive_227()
    {
        var plantUmlText = """
            @startuml ExampleSm
            state c1 <<choice>>
            state c2 <<Choice>>
            state c3 <<CHOICE>>
            [*] --> c1
            c1 --> c2
            c2 -> c3
            c3 --> S1
            @enduml
            """;
        IServiceProvider serviceProvider = TestHelper.CreateServiceProvider();
        InputSmBuilder inputSmBuilder = serviceProvider.GetRequiredService<InputSmBuilder>();
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices("foo.puml", plantUmlText);
        inputSmBuilder.FinishRunning();

        StateMachine root = inputSmBuilder.GetStateMachine();

        // find choice points by diagram id
        root.ChildWithDiagramId("c1").As<ChoicePoint>(); // will throw if not found, or wrong type
        root.ChildWithDiagramId("c2").As<ChoicePoint>(); // will throw if not found, or wrong type
        root.ChildWithDiagramId("c3").As<ChoicePoint>(); // will throw if not found, or wrong type
    }

    [Fact]
    public void UnescapeNewlines()
    {
        // input modified from https://plantuml.com/state-diagram#3b0649c72650c313
        ParseAssertNoError("""
            @startuml SomeSmName
            s1 :  / { initial_tx_action(); \n x++; }
            [*]-->s1:[\n guard1\n && guard2 ]
            @enduml
            """);

        translator.Edges[0].label.Should().Be("""
            [
             guard1
             && guard2 ]
            """);
        translator.Root.children[0].label.Should().Be("""
            s1
            / { initial_tx_action(); 
             x++; }
            """);
    }

    [Fact]
    public void LotsOfNotesAndComments()
    {
        ParseAssertNoError("""
            @startuml ButtonSm1Cpp

            state BetweenNotes1

            note "This is a PlantUML diagram" as N1

            state BetweenNotes2

            note left of Active : this is a short\nnote

            state BetweenNotes3

            note right of Inactive
              A note can also
              state DontFindMe1
            state DontFindMe2
            end note

            'state DontFindMe2

                state BetweenNotes4

                    note right of Inactive
                      A note can also
                      state DontFindMe1
                        state DontFindMe2
                    end note

            state BetweenNotes5

              note right of Inactive
                A note can also
                state DontFindMe1
            state DontFindMe2
              endnote

            state BetweenNotes6

            /'
            Shouldn't find this
            state DontFindMe1
            state DontFindMe2

            '/

                /'
                Shouldn't find this
                state DontFindMe1
                state DontFindMe2
                '/


            state BetweenNotes7

            BetweenNotes1 --> BetweenNotes2
            note on link
              This is a note on a link.
              See https://github.com/StateSmith/StateSmith/issues/343
            end note

            @enduml
            """);

        for (int i = 1; i <= 7; i++)
        {
            GetNodeById("BetweenNotes" + i);
        }

        Action a;
        a = () => GetNodeById("DontFindMe1");
        a.Should().Throw<Exception>();
        a = () => GetNodeById("DontFindMe2");
        a.Should().Throw<Exception>();
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/343
    /// </summary>
    [Fact]
    public void NoteOnLinkKeywords_343()
    {
        // introduced lexer keywords 'on' and 'link'.
        // Need to ensure that they can be used as regular identifiers.

        var plantUmlText = """
            @startuml ExampleSm
            [*] --> on
            on --> link : link / stuff(on)
            link: on / link stuff 2
            @enduml
            """;
        IServiceProvider serviceProvider = TestHelper.CreateServiceProvider();
        InputSmBuilder inputSmBuilder = serviceProvider.GetRequiredService<InputSmBuilder>();
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices("foo.puml", plantUmlText);
        inputSmBuilder.FinishRunning();

        var sm = inputSmBuilder.GetStateMachine();
        sm.ChildWithDiagramId("link").Behaviors.Single().DescribeAsUml().Should().Be("on / { link stuff 2 }");
        sm.ChildWithDiagramId("on").Behaviors.Single().DescribeAsUml().Should().Be("link / { stuff(on) } TransitionTo(link)");
    }

    [Fact]
    public void SkinparamBlock()
    {
        ParseAssertNoError("""
            @startuml blinky1_printf_sm
            skinparam state {
            }

            @enduml
            """);

        ParseAssertNoError("""
                @startuml blinky1_printf_sm
                skinparam state {

                }
                @enduml
            """);

        ParseAssertNoError("""

            @startuml blinky1_printf_sm
            skinparam state {
            }
            @enduml
                        
            """);

        ParseAssertNoError("""
            @startuml blinky1_printf_sm
            skinparam state
            {
            }
            @enduml
            """);

        ParseAssertNoError("""
            @startuml blinky1_printf_sm
            skinparam state
                    
            {
            }
            @enduml
            """);

        ParseAssertNoError("""
            @startuml blinky1_printf_sm
            skinparam state {
             BorderColor<<on_style>> #AA0000
             BackgroundColor<<on_style>> #ffcccc
             FontColor<<on_style>> darkred
             
             BorderColor Black
            }

            @enduml
            """);

        ParseAssertNoError("""
            @startuml blinky1_printf_sm
            skinparam state {

                BorderColor<<on_style>> #AA0000
                BackgroundColor<<on_style>> #ffcccc
                FontColor<<on_style>> darkred
             
                BorderColor Black

            }
            @enduml
            """);

        ParseAssertNoError("""
            @startuml blinky1_printf_sm
            skinparam state {
                BackgroundColor<<on_style>> #ffcccc
            '}
            }

            @enduml
            """);
    }

    [Fact]
    public void SkinparamBlockBad()
    {
        ParseAssertHasAtLeastOneError("""
            @startuml blinky1_printf_sm
            skinparam state {
                BackgroundColor<<on_style>> #ffcccc
                [*] --> B
            }
            @enduml
            """);
    }

    [Fact]
    public void SkinparamBlockBad2()
    {
        ParseAssertHasAtLeastOneError("""
            @startuml blinky1_printf_sm
            skinparam state {
                BackgroundColor<<on_style>> #ffcccc
            }'
            LED_ON : enter / { action(); }
            @enduml
            """);
    }

    /// <summary>
    /// See https://github.com/StateSmith/StateSmith/issues/216
    /// Ignore title line
    /// </summary>
    [Fact]
    public void DiagramIgnoreTitle_216()
    {
        ParseAssertNoError("""
            @startuml MyPumlSm1
            title Test Title
            @enduml
            """);

        ParseAssertNoError("""
            @startuml MyPumlSm1
            title ""Test Title""
            @enduml
            """);

        ParseAssertNoError("""
            @startuml MyPumlSm1
            title //**Test// Title**
            @enduml
            """);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/216
    /// We shouldn't ignore title if it is used as a state name
    /// </summary>
    [Fact]
    public void TitleStateName_216()
    {
        ParseAssertNoError("""
            @startuml SomeName
            title --> S1
            [*] --> mainframe
            @enduml
            """);

        GetNodeById("title");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/216
    /// We shouldn't ignore title if it is used as a state name
    /// </summary>
    [Fact]
    public void TitleStateName2_216()
    {
        ParseAssertNoError("""
            @startuml SomeName
            S1 --> title
            [*] --> S1
            @enduml
            """);

        GetNodeById("title");
    }

    //###############################################################################################
    // Start of tests for https://github.com/StateSmith/StateSmith/issues/215
    //###############################################################################################

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/215
    /// </summary>
    [Fact]
    public void IgnoreMainframeSimple_215()
    {
        ParseAssertNoError("""
            @startuml blinky1_printf_sm
            mainframe This is a **mainframe**
            @enduml
            """);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/215
    /// </summary>
    [Fact]
    public void IgnoreMainframeStartingWithLetter_215()
    {
        ParseAssertNoError("""
            @startuml SomeName
            mainframe This is a **mainframe** and other characters 12398753298-->:!@(*&^%##$%^&*()_+{}|:"<>?[]\;',./
            [*] --> S1
            @enduml
            """);

        ParseAssertNoError("""
            @startuml SomeName
            mainframe lowercase title and other characters 12398753298-->:!@(*&^%##$%^&*()_+{}|:"<>?[]\;',./
            [*] --> S1
            @enduml
            """);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/215
    /// </summary>
    [Fact]
    public void IgnoreMainframeStartingWithDigit_215()
    {
        ParseAssertNoError("""
            @startuml SomeName
            mainframe 1234 This is a **mainframe** and other characters 12398753298-->:!@(*&^%##$%^&*()_+{}|:"<>?[]\;',./
            [*] --> S1
            @enduml
            """);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/215
    /// </summary>
    [Fact]
    public void IgnoreMainframeStartingWithBold_215()
    {
        ParseAssertNoError("""
            @startuml SomeName
            mainframe **1234 title**
            [*] --> S1
            @enduml
            """);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/215
    /// </summary>
    [Fact]
    public void IgnoreMainframeStartingWithItalic_215()
    {
        ParseAssertNoError("""
            @startuml SomeName
            mainframe //**1234 title**//
            [*] --> S1
            @enduml
            """);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/215
    /// We shouldn't ignore mainframe if it is used as a state name
    /// </summary>
    [Fact]
    public void MainframeStateName_215()
    {
        ParseAssertNoError("""
            @startuml SomeName
            mainframe --> S1
            [*] --> mainframe
            @enduml
            """);

        GetNodeById("mainframe");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/215
    /// skin rose https://twitter.com/PlantUML/status/1492968858960990222?lang=en
    /// </summary>
    [Fact]
    public void IgnoreSkinRoseTiny_215()
    {
        ParseAssertNoError("""
            @startuml blinky1_printf_sm
            skin rose
            @enduml
            """);
    }

    /// <summary>
    /// skin basic https://github.com/StateSmith/StateSmith/issues/215
    /// </summary>
    [Fact]
    public void IgnoreSkinBasicTiny_215()
    {
        ParseAssertNoError("""
            @startuml SomeName
            skin basic
            [*] --> S1
            @enduml
            """);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/215
    /// We shouldn't ignore skin if it is used as a state name
    /// </summary>
    [Fact]
    public void SkinStateName_215()
    {
        ParseAssertNoError("""
            @startuml SomeName
            skin --> S1
            [*] --> skin
            @enduml
            """);

        GetNodeById("skin");
    }

    //###############################################################################################
    // End of tests for https://github.com/StateSmith/StateSmith/issues/215
    //###############################################################################################



    //###############################################################################################
    // Start of tests for https://github.com/StateSmith/StateSmith/issues/334
    //###############################################################################################

    [Fact]
    public void TomlConfig_334()
    {
        ParseAssertNoError("""
            @startuml SomeName
            [*] --> first

            /' a normal ignored comment block '/
            /'! $CONFIG : toml
            [RenderConfig]
            FileTop = "// Copyright blah blah"
            
            '/

            @enduml
            """);

        translator.Root.children.Should().HaveCount(3);
        var tomlConfigNode = translator.Root.children[2];
        tomlConfigNode.label.ShouldBeShowDiff("""
            $CONFIG : toml
            [RenderConfig]
            FileTop = "// Copyright blah blah"
            """);
    }

    //###############################################################################################
    // End of tests for https://github.com/StateSmith/StateSmith/issues/334
    //###############################################################################################

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/424
    /// </summary>
    [Fact]
    public void EntryExitPoint_FromEvent_424()
    {
        var plantUmlText = """
            @startuml SomeName
            [*] --> StateA
            
            state Idle {
                state ToOffEntryPt <<entryPoint>>
                state ToOnEntryPt <<entryPoint>>
                state ToStateAExitPt <<exitPoint>>
                state Off
                state On
                ToOffEntryPt --> Off
                ToOnEntryPt --> On
                On --> ToStateAExitPt : REASON1
                ToStateAExitPt --> StateA
            }
            
            StateA --> ToOnEntryPt : REASON1
            StateA --> ToOffEntryPt : REASON2
            
            @enduml
            """;

        IServiceProvider serviceProvider = TestHelper.CreateServiceProvider();
        InputSmBuilder inputSmBuilder = serviceProvider.GetRequiredService<InputSmBuilder>();
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices("foo.puml", plantUmlText);
        inputSmBuilder.FinishRunning();

        StateMachine root = inputSmBuilder.GetStateMachine();
        State StateA = root.Child<State>("StateA");
        State StateOn = (State)root.DescendantWithDiagramId("On");

        // test transitions into and out of entry/exit points

        {
            EntryPoint ToOnEntryPt = (EntryPoint)root.DescendantWithDiagramId("ToOnEntryPt");
            ToOnEntryPt.IncomingTransitions.Single().OwningVertex.Should().Be(StateA);
            ToOnEntryPt.IncomingTransitions.Single().DescribeAsUml().ShouldBeShowDiff("""
                REASON1 TransitionTo(Idle.<EntryPoint>(ToOnEntryPt))
                """);

            ToOnEntryPt.TransitionBehaviors().Single().DescribeAsUml().ShouldBeShowDiff("""
                TransitionTo(On)
                """);
        }

        {
            EntryPoint ToOffEntryPt = (EntryPoint)root.DescendantWithDiagramId("ToOffEntryPt");
            ToOffEntryPt.IncomingTransitions.Single().OwningVertex.Should().Be(StateA);
            ToOffEntryPt.IncomingTransitions.Single().DescribeAsUml().ShouldBeShowDiff("""
                REASON2 TransitionTo(Idle.<EntryPoint>(ToOffEntryPt))
                """);

            ToOffEntryPt.TransitionBehaviors().Single().DescribeAsUml().ShouldBeShowDiff("""
                TransitionTo(Off)
                """);
        }

        {
            ExitPoint ToStateAExitPt = (ExitPoint)root.DescendantWithDiagramId("ToStateAExitPt");
            ToStateAExitPt.IncomingTransitions.Single().OwningVertex.Should().Be(StateOn);
            ToStateAExitPt.IncomingTransitions.Single().DescribeAsUml().ShouldBeShowDiff("""
                REASON1 TransitionTo(Idle.<ExitPoint>(ToStateAExitPt))
                """);

            ToStateAExitPt.TransitionBehaviors().Single().DescribeAsUml().ShouldBeShowDiff("""
                TransitionTo(StateA)
                """);
        }
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/460
    /// </summary>
    [Fact]
    public void RedeclaredNodeInsideAnotherNode_Explicit_460()
    {
        // The below doesn't make sense because CONFIG is redeclared from root to MENU.
        var plantUmlText = """
            @startuml SomeName
            state CONFIG
            
            state MENU {
                state CONFIG
            }

            [*] --> CONFIG
            @enduml
            """;

        var action = () => ParseAssertNoError("foo.puml", plantUmlText);

        action.Should().Throw<Exception>()
            .WithMessage("*'CONFIG' cannot be re-declared inside another node 'MENU'*https://github.com/StateSmith/StateSmith/issues/460*");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/460
    /// </summary>
    [Fact]
    public void RedeclaredNodeInsideAnotherNode_Implicit_460()
    {
        // The below doesn't make sense because CONFIG is redeclared from root to MENU.
        var plantUmlText = """
            @startuml SomeName
            [*] --> CONFIG
            
            state MENU {
                state CONFIG
            }
            @enduml
            """;

        var action = () => ParseAssertNoError("foo.puml", plantUmlText);

        action.Should().Throw<Exception>()
            .WithMessage("*'CONFIG' cannot be re-declared inside another node 'MENU'*https://github.com/StateSmith/StateSmith/issues/460*");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/460
    /// </summary>
    [Fact]
    public void RedeclaredNodeAtRootLevel_460()
    {
        var plantUmlText = """
            @startuml SomeName
            state MENU {
                state CONFIG
            }

            state CONFIG {
                state CONFIG_INNER
            }

            [*] --> CONFIG

            @enduml
            """;

        IServiceProvider serviceProvider = TestHelper.CreateServiceProvider();
        InputSmBuilder inputSmBuilder = serviceProvider.GetRequiredService<InputSmBuilder>();
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices("foo.puml", plantUmlText);
        inputSmBuilder.FinishRunning();

        StateMachine root = inputSmBuilder.GetStateMachine();
        root.Children.Count.Should().Be(2); // MENU and initial state
        root.ChildType<InitialState>(); // requires only 1
        var menu = root.ChildType<State>(); // requires only 1

        menu.Name.Should().Be("MENU");
        var config = menu.ChildType<State>();
        config.Name.Should().Be("CONFIG");
        menu.Children.Count.Should().Be(1);

        config.Children.Count.Should().Be(1);
        var configInner = config.ChildType<State>();
        configInner.Name.Should().Be("CONFIG_INNER");
        configInner.Children.Count.Should().Be(0);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/460
    /// </summary>
    [Fact]
    public void RedeclaredNodeAllowed_460()
    {
        // before improvement for #460, below PANIC would end up nested in MENU state instead of root state machine.
        // Same with initial state `[*] --> MENU`
        var plantUmlText = """
            @startuml SomeName
            state MENU {
                [*] -> MENU__DISARM_SYSTEM
                state MENU__DISARM_SYSTEM
                state DISARM_SYSTEM
                state ARM_SYSTEM
                state CONFIG {
                    [*] -> CONFIG__CHANGE_CODE
                    state CONFIG__CHANGE_CODE
                }
            }

            state CONFIG {
                state SECRET_MENU
            }
            state PANIC
            [*] --> MENU

            @enduml
            """;

        IServiceProvider serviceProvider = TestHelper.CreateServiceProvider();
        InputSmBuilder inputSmBuilder = serviceProvider.GetRequiredService<InputSmBuilder>();
        inputSmBuilder.ConvertPlantUmlTextNodesToVertices("foo.puml", plantUmlText);
        inputSmBuilder.FinishRunning();

        StateMachine root = inputSmBuilder.GetStateMachine();
        root.ChildWithDiagramId("PANIC").As<State>().Parent.Should().Be(root);
    }

    private DiagramNode GetNodeById(string id)
    {
        return translator.GetDiagramNode(id);
    }

    private static void AssertEdge(DiagramEdge diagramEdge, DiagramNode source, DiagramNode target)
    {
        diagramEdge.source.Should().Be(source);
        diagramEdge.target.Should().Be(target);
    }

    private static void AssertEdge(DiagramEdge diagramEdge, DiagramNode source, DiagramNode target, string label)
    {
        AssertEdge(diagramEdge, source, target);
        diagramEdge.label.Should().Be(label);
    }
}

