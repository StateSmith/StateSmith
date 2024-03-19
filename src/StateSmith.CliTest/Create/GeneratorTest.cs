using FluentAssertions;
using NSubstitute;
using StateSmith.Cli.Create;
using StateSmith.Cli.Utils;
using StateSmithTest;
using Xunit;

namespace StateSmithCliTest.Create;

public class GeneratorTest
{
    [Fact]
    public void DrawioTest()
    {
        const string CsxFilePath = "RocketCodeGen.csx";
        const string DiagramPath = "RocketDiagram.drawio";

        var settings = new Settings
        {
            diagramFileName = DiagramPath,
            scriptFileName = CsxFilePath,
            TargetLanguageId = TargetLanguageId.C,
            StateSmithVersion = "0.9.9-alpha",
            FileExtension = ".drawio",
            DrawIoDiagramTemplateId = "_test-drawio-1",
            smName = "RocketSm"
        };

        var mockFileWriter = Substitute.For<IFileWriter>();
        Generator generator = new(settings);
        generator.SetFileWriter(mockFileWriter);

        // NSubsitute doesn't diff large strings very well, so we use ShouldBeShowDiff to show the differences
        mockFileWriter.When(x => x.Write(CsxFilePath, Arg.Any<string>())).Do(x => {
            x.ArgAt<string>(1).ShouldBeShowDiff("""
            #!/usr/bin/env dotnet-script
            #r "nuget: StateSmith, 0.9.9-alpha"
            SmRunner runner = new(diagramPath: "RocketDiagram.drawio", new MyRenderConfig(), transpilerId: TranspilerId.C99);

            """);
        });

        // NSubsitute doesn't diff large strings very well, so we use ShouldBeShowDiff to show the differences
        mockFileWriter.When(x => x.Write(DiagramPath, Arg.Any<string>())).Do(x => {
            x.ArgAt<string>(1).ShouldBeShowDiff("""
            <mxCell id="12" value="$STATEMACHINE : RocketSm" ...stuff...>

            """);
        });

        generator.GenerateFiles();

        // make sure the calls were made
        mockFileWriter.Received().Write(CsxFilePath, Arg.Any<string>());
        mockFileWriter.Received().Write(DiagramPath, Arg.Any<string>());
    }

    [Fact]
    public void DrawioSvgTest()
    {
        const string CsxFilePath = "RocketCodeGen.csx";
        const string DiagramPath = "RocketDiagram.drawio";

        var settings = new Settings
        {
            diagramFileName = DiagramPath,
            scriptFileName = CsxFilePath,
            TargetLanguageId = TargetLanguageId.C,
            StateSmithVersion = "0.9.9-alpha",
            FileExtension = ".drawio.svg",
            DrawIoDiagramTemplateId = "_test-drawio-1",
            smName = "RocketSm"
        };

        var mockFileWriter = Substitute.For<IFileWriter>();
        Generator generator = new(settings);
        generator.SetFileWriter(mockFileWriter);

        // NSubsitute doesn't diff large strings very well, so we use ShouldBeShowDiff to show the differences
        mockFileWriter.When(x => x.Write(CsxFilePath, Arg.Any<string>())).Do(x => {
            x.ArgAt<string>(1).ShouldBeShowDiff("""
            #!/usr/bin/env dotnet-script
            #r "nuget: StateSmith, 0.9.9-alpha"
            SmRunner runner = new(diagramPath: "RocketDiagram.drawio", new MyRenderConfig(), transpilerId: TranspilerId.C99);

            """);
        });

        // NSubsitute doesn't diff large strings very well, so we use ShouldBeShowDiff to show the differences
        mockFileWriter.When(x => x.Write(DiagramPath, Arg.Any<string>())).Do(x => {
            var content = x.ArgAt<string>(1);
            content.Should().Contain("value=&quot;$STATEMACHINE : RocketSm&quot;");
        });

        generator.GenerateFiles();

        // make sure the calls were made
        mockFileWriter.Received().Write(CsxFilePath, Arg.Any<string>());
        mockFileWriter.Received().Write(DiagramPath, Arg.Any<string>());
    }

    [Fact]
    public void PlantumlTest()
    {
        const string CsxFilePath = "../../TrafficCodeGen.csx";
        const string DiagramPath = "./a/b/c/TrafficDiagram.plantuml";

        var settings = new Settings
        {
            diagramFileName = DiagramPath,
            scriptFileName = CsxFilePath,
            TargetLanguageId = TargetLanguageId.JavaScript,
            StateSmithVersion = "0.9.7-alpha",
            FileExtension = ".plantuml",
            PlantUmlDiagramTemplateId = "_test-plantuml-1",
            smName = "TrafficStateMachine"
        };

        var mockFileWriter = Substitute.For<IFileWriter>();
        Generator generator = new(settings);
        generator.SetFileWriter(mockFileWriter);

        // NSubsitute doesn't diff large strings very well, so we use ShouldBeShowDiff to show the differences
        mockFileWriter.When(x => x.Write(CsxFilePath, Arg.Any<string>())).Do(x => {
            x.ArgAt<string>(1).ShouldBeShowDiff("""
            #!/usr/bin/env dotnet-script
            #r "nuget: StateSmith, 0.9.7-alpha"
            SmRunner runner = new(diagramPath: "./a/b/c/TrafficDiagram.plantuml", new MyRenderConfig(), transpilerId: TranspilerId.JavaScript);

            """);
        });

        // NSubsitute doesn't diff large strings very well, so we use ShouldBeShowDiff to show the differences
        mockFileWriter.When(x => x.Write(DiagramPath, Arg.Any<string>())).Do(x => {
            x.ArgAt<string>(1).ShouldBeShowDiff("""
            @startuml TrafficStateMachine
            @enduml
            
            """);
        });

        generator.GenerateFiles();

        // make sure the calls were made
        mockFileWriter.Received().Write(CsxFilePath, Arg.Any<string>());
        mockFileWriter.Received().Write(DiagramPath, Arg.Any<string>());
    }
}
