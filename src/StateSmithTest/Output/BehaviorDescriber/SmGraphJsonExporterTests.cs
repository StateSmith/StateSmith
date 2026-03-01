#nullable enable

using System;
using FluentAssertions;
using StateSmith.Output;
using StateSmith.Runner;
using StateSmith.SmGraph;
using Xunit;

namespace StateSmithTest.Output.SmDescriberTest;

// https://github.com/StateSmith/StateSmith/issues/528
public class SmGraphJsonExporterTests
{
    [Fact]
    public void IntegrationTest_FileOutput()
    {
        var smName = "RocketSm_" + Guid.NewGuid().ToString().Replace('-', '_');

        var plantUmlText = $"""
            @startuml {smName}
            [*] --> s1

            /'! $CONFIG : toml
                SmRunnerSettings.smGraphJsonExporter.enabled = true
            '/
            @enduml
            """;

        var fakeFs = new CapturingCodeFileWriter();
        var jsonFileName = smName + ".export.json";
        var fileName = smName + ".plantuml";

        var tmpDir = TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter:fakeFs, fileName: fileName, transpilerId:TranspilerId.JavaScript);
        fakeFs.GetSoleCaptureWithName(jsonFileName).filePath.Should().Be($"{tmpDir}/{jsonFileName}");
    }

    [Fact]
    public void IntegrationTest_FileOutputRelative_AndPostfix()
    {
        var smName = "RocketSm_" + Guid.NewGuid().ToString().Replace('-', '_');

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

        var fakeFs = new CapturingCodeFileWriter();
        var jsonFileName = smName + ".json";
        var fileName = smName + ".plantuml";

        var tmpDir = TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter:fakeFs, fileName: fileName, transpilerId:TranspilerId.JavaScript);
        CapturingCodeFileWriter.Capture capture = fakeFs.GetSoleCaptureWithName(jsonFileName);
        capture.filePath.Should().Be($"{tmpDir}/meta/sub1/sub2/{jsonFileName}");

        // sections are present by default
        capture.code.Should().Contain("beforeTransformations");
        capture.code.Should().Contain("afterTransformations");
    }

    [Fact]
    public void IntegrationTest_DisableBeforeAfterTransformations()
    {
        var smName = "RocketSm_" + Guid.NewGuid().ToString().Replace('-', '_');

        var plantUmlText = $"""
            @startuml {smName}
            [*] --> s1

            /'! $CONFIG : toml
                SmRunnerSettings.smGraphJsonExporter.enabled = true
                SmRunnerSettings.smGraphJsonExporter.beforeTransformations = false
                SmRunnerSettings.smGraphJsonExporter.afterTransformations = false
            '/
            @enduml
            """;

        var fakeFs = new CapturingCodeFileWriter();
        var jsonFileName = smName + ".export.json";
        var fileName = smName + ".plantuml";

        var tmpDir = TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter:fakeFs, fileName: fileName, transpilerId:TranspilerId.JavaScript);
        CapturingCodeFileWriter.Capture capture = fakeFs.GetSoleCaptureWithName(jsonFileName);
        
        capture.code.Should().NotContain("beforeTransformations");
        capture.code.Should().NotContain("afterTransformations");
    }

    // actually writes to file so that we can get real console messages. If we were to use a fake file writer, we don't see printed messages.
    [Fact]
    public void IntegrationTestConsoleMessage()
    {
        var smName = "RocketSm_" + Guid.NewGuid().ToString().Replace('-', '_');

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
        var jsonFileName = smName + ".export.json";
        var fileName = smName + ".plantuml";

        var tmpDir = TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, useRealFileWriter: true, consoleCapturer: console, fileName: fileName, transpilerId:TranspilerId.JavaScript);

        var printedConsole = console.sb.ToString();
        printedConsole.Should().Contain($"Writing to file `meta/{jsonFileName}`");
    }

    [Fact]
    public void BeforeTransformations()
    {
        SmGraphJsonExporter exporter = new();
        exporter.RecordBeforeTransformations(new StateMachine("RocketSm"));
        exporter.ExportToJson().ShouldBeShowDiff("""
            {
              "comment": "NOTE! Json export in alpha. Feedback welcome. See https://github.com/StateSmith/StateSmith/issues/528",
              "beforeTransformations": [
                {
                  "nodeId": "<StateMachine>(RocketSm)",
                  "type": "StateMachine",
                  "name": "RocketSm",
                  "diagramId": ""
                }
              ]
            }
            """);
    }

    [Fact]
    public void AfterTransformations()
    {
        SmGraphJsonExporter exporter = new();
        exporter.RecordAfterTransformations(new StateMachine("RocketSm"));
        exporter.ExportToJson().ShouldBeShowDiff("""
            {
              "comment": "NOTE! Json export in alpha. Feedback welcome. See https://github.com/StateSmith/StateSmith/issues/528",
              "afterTransformations": [
                {
                  "nodeId": "<StateMachine>(RocketSm)",
                  "type": "StateMachine",
                  "name": "RocketSm",
                  "diagramId": ""
                }
              ]
            }
            """);
    }

    [Fact]
    public void BeforeAndAfterTransformations()
    {
        SmGraphJsonExporter exporter = new();
        exporter.RecordAfterTransformations(new StateMachine("RocketSm"));
        exporter.RecordBeforeTransformations(new StateMachine("MySm"));

        exporter.ExportToJson().ShouldBeShowDiff("""
            {
              "comment": "NOTE! Json export in alpha. Feedback welcome. See https://github.com/StateSmith/StateSmith/issues/528",
              "beforeTransformations": [
                {
                  "nodeId": "<StateMachine>(MySm)",
                  "type": "StateMachine",
                  "name": "MySm",
                  "diagramId": ""
                }
              ],
              "afterTransformations": [
                {
                  "nodeId": "<StateMachine>(RocketSm)",
                  "type": "StateMachine",
                  "name": "RocketSm",
                  "diagramId": ""
                }
              ]
            }
            """);
    }

    [Fact]
    public void ExactWhitespaceTest()
    {
        SmGraphJsonExporter exporter = new();
        exporter.RecordBeforeTransformations(BuildTestSm());

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

    public static StateMachine BuildTestSm()
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
}
