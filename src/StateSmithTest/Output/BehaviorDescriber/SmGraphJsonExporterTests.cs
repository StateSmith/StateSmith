#nullable enable

using System;
using FluentAssertions;
using StateSmith.Output;
using StateSmith.Runner;
using StateSmith.SmGraph;
using Xunit;

namespace StateSmithTest.Output.SmGraphJsonExporterTests;

// https://github.com/StateSmith/StateSmith/issues/528
public class IntegrationTests
{
    private const string AfterTransformationsKey = "afterTransformations";
    private const string BeforeTransformationsKey = "beforeTransformations";

    // use a unique name to avoid parallel test issues
    readonly string smName = TestHelper.GenerateUniqueSmName();
    string jsonFileName;

    public IntegrationTests()
    {
        jsonFileName = smName + ".export.json";
    }

    // actually writes to file so that we can get real console messages. If we were to use a fake file writer, we don't see printed messages.
    [Fact]
    public void IntegrationTestConsoleMessage()
    {
        var plantUmlText = $"""
            @startuml {smName}
            [*] --> s1

            /'! $CONFIG : toml
                SmRunnerSettings.smGraphJsonExporter.enabled = true
                SmRunnerSettings.smGraphJsonExporter.outputDirectory = "meta"
            '/
            @enduml
            """;

        var console = new StringBuilderConsolePrinter();
        TestHelper.CaptureNonCodeGenRunSmRunnerForPlantUmlString(plantUmlText, useRealFileWriter: true, consoleCapturer: console, transpilerId:TranspilerId.JavaScript);

        var printedConsole = console.sb.ToString();
        printedConsole.Should().Contain($"Writing to file `meta/{jsonFileName}`");
    }

    [Fact]
    public void DisabledByDefault()
    {
        var plantUmlText = $"""
            @startuml {smName}
            [*] --> s1

            /'! $CONFIG : toml
            '/
            @enduml
            """;

        var fakeFs = new CapturingCodeFileWriter();
        TestHelper.CaptureNonCodeGenRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter:fakeFs, transpilerId:TranspilerId.JavaScript);
        
        // no captures should contain .json
        fakeFs.captures.GetKeys().Should().NotContain(k => k.Contains(".json"));
    }

    public (CapturingCodeFileWriter.Capture, string relativeDir) IntegrationTestPlantUml(string plantuml)
    {
        var fakeFs = new CapturingCodeFileWriter();
        var relativeDir = TestHelper.CaptureNonCodeGenRunSmRunnerForPlantUmlString(plantuml, codeFileWriter:fakeFs, transpilerId:TranspilerId.JavaScript);
        CapturingCodeFileWriter.Capture fileCapture = fakeFs.GetSoleCaptureWithName(jsonFileName);

        return (fileCapture, relativeDir);
    }

    [Fact]
    public void FileOutput()
    {
        var plantUmlText = $"""
            @startuml {smName}
            [*] --> s1

            /'! $CONFIG : toml
                SmRunnerSettings.smGraphJsonExporter.enabled = true
            '/
            @enduml
            """;

        var (fileCapture, relativeDir) = IntegrationTestPlantUml(plantUmlText);
        fileCapture.filePath.Should().Be($"{relativeDir}/{jsonFileName}");
    }

    [Fact]
    public void FileOutputRelative_AndPostfix()
    {
        jsonFileName = smName + ".json";

        var plantUmlText = $"""
            @startuml {smName}
            [*] --> s1

            /'! $CONFIG : toml
                SmRunnerSettings.smGraphJsonExporter.enabled = true
                SmRunnerSettings.smGraphJsonExporter.outputDirectory = "meta/sub1/sub2"
                SmRunnerSettings.smGraphJsonExporter.outputFileNamePostfix = ".json"
            '/
            @enduml
            """;

        var (fileCapture, relativeDir) = IntegrationTestPlantUml(plantUmlText);
        fileCapture.filePath.Should().Be($"{relativeDir}/meta/sub1/sub2/{jsonFileName}");

        // sections are present by default
        fileCapture.code.Should().Contain(BeforeTransformationsKey);
        fileCapture.code.Should().Contain(AfterTransformationsKey);
    }

    [Fact]
    public void DisableBeforeTransformations()
    {
        var plantUmlText = $"""
            @startuml {smName}
            [*] --> s1

            /'! $CONFIG : toml
                SmRunnerSettings.smGraphJsonExporter.enabled = true
                SmRunnerSettings.smGraphJsonExporter.beforeTransformations = false
            '/
            @enduml
            """;

        var (fileCapture, _) = IntegrationTestPlantUml(plantUmlText);
        fileCapture.code.Should().NotContain(BeforeTransformationsKey);
        fileCapture.code.Should().Contain(AfterTransformationsKey);
    }

    [Fact]
    public void DisableAfterTransformations()
    {
        var plantUmlText = $"""
            @startuml {smName}
            [*] --> s1

            /'! $CONFIG : toml
                SmRunnerSettings.smGraphJsonExporter.enabled = true
                SmRunnerSettings.smGraphJsonExporter.afterTransformations = false
            '/
            @enduml
            """;

        var (fileCapture, _) = IntegrationTestPlantUml(plantUmlText);
        fileCapture.code.Should().Contain(BeforeTransformationsKey);
        fileCapture.code.Should().NotContain(AfterTransformationsKey);
    }
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


// https://github.com/StateSmith/StateSmith/issues/528
public class UnitTests
{
    SmGraphJsonExporter exporter = new();

    SmGraphJsonExporterSettings settings = new()
    {
        enabled = true
    };

    [Fact]
    public void BeforeTransformations()
    {
        settings.afterTransformations = false;

        exporter.RecordBeforeTransformations(settings, new StateMachine("BeforeSm"));
        exporter.RecordAfterTransformations(settings, new StateMachine("AfterSm"));
        exporter.ExportToJson().ShouldBeShowDiff("""
            {
              "comment": "NOTE! Json export in alpha. Feedback welcome. See https://github.com/StateSmith/StateSmith/issues/528",
              "beforeTransformations": [
                {
                  "nodeId": "<StateMachine>(BeforeSm)",
                  "type": "StateMachine",
                  "name": "BeforeSm",
                  "diagramId": ""
                }
              ]
            }
            """);
    }

    [Fact]
    public void AfterTransformations()
    {
        settings.beforeTransformations = false;

        exporter.RecordBeforeTransformations(settings, new StateMachine("BeforeSm"));
        exporter.RecordAfterTransformations(settings, new StateMachine("AfterSm"));
        exporter.ExportToJson().ShouldBeShowDiff("""
            {
              "comment": "NOTE! Json export in alpha. Feedback welcome. See https://github.com/StateSmith/StateSmith/issues/528",
              "afterTransformations": [
                {
                  "nodeId": "<StateMachine>(AfterSm)",
                  "type": "StateMachine",
                  "name": "AfterSm",
                  "diagramId": ""
                }
              ]
            }
            """);
    }

    [Fact]
    public void BeforeAndAfterTransformations()
    {
        exporter.RecordBeforeTransformations(settings, new StateMachine("BeforeSm"));
        exporter.RecordAfterTransformations(settings, new StateMachine("AfterSm"));

        exporter.ExportToJson().ShouldBeShowDiff("""
            {
              "comment": "NOTE! Json export in alpha. Feedback welcome. See https://github.com/StateSmith/StateSmith/issues/528",
              "beforeTransformations": [
                {
                  "nodeId": "<StateMachine>(BeforeSm)",
                  "type": "StateMachine",
                  "name": "BeforeSm",
                  "diagramId": ""
                }
              ],
              "afterTransformations": [
                {
                  "nodeId": "<StateMachine>(AfterSm)",
                  "type": "StateMachine",
                  "name": "AfterSm",
                  "diagramId": ""
                }
              ]
            }
            """);
    }

    [Fact]
    public void MoreFullTest()
    {
        exporter.RecordBeforeTransformations(settings, BuildTestSm());

        static StateMachine BuildTestSm()
        {
            var sm = new StateMachine("MySm")
            {
                DiagramId = "123"
            };
            sm.AddEnterAction("sm_enter();");
            sm.AddBehavior(new Behavior(trigger: "SomeEvent", guardCode:"x < 44", actionCode: "x += 66;"));

            var s1 = sm.AddChild(new State("S1")
            {
                DiagramId = "456",
            });
            s1.AddEnterAction("s1_enter();");
            s1.AddBehavior(new Behavior(trigger: "ev1", actionCode: "s1_ev1_stuff();", transitionTarget: s1)).order = 1;

            // purposely use same name as parent to show how non-unique names are handled.
            s1.AddChild(new State("S1")
            {
                DiagramId = "789",
            });

            // add initial state with initial transition
            var initialState = sm.AddChild(new InitialState());
            initialState.AddBehavior(new Behavior(transitionTarget: s1));

            return sm;
        }

        // note nodeId "S1.S1" showing how non-unique state names are handled.

        var json = exporter.ExportToJson();
        json.ShouldBeShowDiff("""
            {
              "comment": "NOTE! Json export in alpha. Feedback welcome. See https://github.com/StateSmith/StateSmith/issues/528",
              "beforeTransformations": [
                {
                  "nodeId": "<StateMachine>(MySm)",
                  "type": "StateMachine",
                  "name": "MySm",
                  "diagramId": "123",
                  "childrenIds": [
                    "S1",
                    "ROOT.<InitialState>"
                  ],
                  "behaviors": [
                    {
                      "triggers": [
                        "enter"
                      ],
                      "actionCode": "sm_enter();"
                    },
                    {
                      "triggers": [
                        "SomeEvent"
                      ],
                      "guardCode": "x < 44",
                      "actionCode": "x += 66;"
                    }
                  ]
                },
                {
                  "nodeId": "S1",
                  "type": "State",
                  "name": "S1",
                  "parentId": "<StateMachine>(MySm)",
                  "diagramId": "456",
                  "childrenIds": [
                    "S1.S1"
                  ],
                  "behaviors": [
                    {
                      "triggers": [
                        "enter"
                      ],
                      "actionCode": "s1_enter();"
                    },
                    {
                      "transitionTargetNodeId": "S1",
                      "triggers": [
                        "ev1"
                      ],
                      "order": 1,
                      "actionCode": "s1_ev1_stuff();"
                    }
                  ]
                },
                {
                  "nodeId": "S1.S1",
                  "type": "State",
                  "name": "S1",
                  "parentId": "S1",
                  "diagramId": "789"
                },
                {
                  "nodeId": "ROOT.<InitialState>",
                  "type": "InitialState",
                  "parentId": "<StateMachine>(MySm)",
                  "diagramId": "",
                  "behaviors": [
                    {
                      "transitionTargetNodeId": "S1"
                    }
                  ]
                }
              ]
            }
            """);
    }
}
