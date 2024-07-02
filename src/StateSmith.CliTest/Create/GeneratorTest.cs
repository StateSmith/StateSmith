using FluentAssertions;
using NSubstitute;
using Spectre.Console;
using StateSmith.Cli.Create;
using StateSmith.Cli.Utils;
using StateSmith.Output;
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
            UseCsxWorkflow = true,
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
            UseCsxWorkflow = true,
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
        const string CsxFilePath = "TrafficCodeGen.csx";
        const string DiagramPath = "TrafficDiagram.plantuml";

        var settings = new Settings
        {
            UseCsxWorkflow = true,
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
            SmRunner runner = new(diagramPath: "TrafficDiagram.plantuml", new MyRenderConfig(), transpilerId: TranspilerId.JavaScript);

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

    [Fact]
    public void DiagramInSiblingDirectory()
    {
        const string CsxPath = "./code_gen/RocketSm.csx";
        const string DiagramPath = "./diagrams/RocketSm.drawio";
        var settings = new Settings
        {
            UseCsxWorkflow = true,
            diagramFileName = DiagramPath,
            scriptFileName = CsxPath,
            TargetLanguageId = TargetLanguageId.JavaScript,
            FileExtension = ".drawio",
            smName = "RocketSm"
        };

        var mockFileWriter = Substitute.For<IFileWriter>();
        Generator generator = new(settings);
        generator.SetFileWriter(mockFileWriter);

        // NSubsitute doesn't diff large strings very well, so we use ShouldBeShowDiff to show the differences
        mockFileWriter.When(x => x.Write(CsxPath, Arg.Any<string>())).Do(x => {
            x.ArgAt<string>(1).Should().Contain("../diagrams/RocketSm.drawio");
        });

        generator.GenerateFiles();

        // make sure the calls were made
        mockFileWriter.Received().Write(CsxPath, Arg.Any<string>());
        mockFileWriter.Received().Write(DiagramPath, Arg.Any<string>());
    }

    [Fact]
    public void TestPlantUmlTomlTemplate()
    {
        var settings = new Settings
        {
            TargetLanguageId = TargetLanguageId.JavaScript,
            FileExtension = ".plantuml",
            PlantUmlDiagramTemplateId = TemplateIds.PlantUmlSimple1,
            smName = "LedSm"
        };

        var generator = new Generator(settings);
        var text = generator.GenerateDiagramFileText();

        text.Should().Contain("@startuml LedSm");
        text.Should().Contain("[RenderConfig.JavaScript]");
    }

    [Fact]
    public void TestDrawioTomlTemplate()
    {
        var settings = new Settings
        {
            TargetLanguageId = TargetLanguageId.C,
            FileExtension = ".drawio",
            PlantUmlDiagramTemplateId = TemplateIds.DrawIoSimple1,
            smName = "LedSm"
        };

        var generator = new Generator(settings);
        var text = generator.GenerateDiagramFileText();

        text.Should().Contain("[RenderConfig]&#10;"); // for new lines
        text.Should().Contain("[RenderConfig.C]");
    }

    /// <summary>
    /// In this simplified workflow, the toml file specifies the transpiler id.
    /// It should not be commented out.
    /// </summary>
    [Fact]
    public void TomlSpecifiesTranspilerId_NoCsx()
    {
        var settings = new Settings
        {
            UseCsxWorkflow = false,
            TargetLanguageId = TargetLanguageId.C,
            FileExtension = ".plantuml", // use plantuml so that line endings aren't replaced for drawio
            PlantUmlDiagramTemplateId = TemplateIds.PlantUmlSimple1,
            smName = "LedSm"
        };

        var generator = new Generator(settings);
        var text = generator.GenerateDiagramFileText();

        text = StringUtils.ConvertToSlashNLines(text); // for below assertions
        text.Should().Contain("\ntranspilerId = \"C99\"");
    }

    /// <summary>
    /// When CSX file is used, the transpiler id should be commented out.
    /// </summary>
    [Fact]
    public void TomlSpecifiesTranspilerId_Csx()
    {
        var settings = new Settings
        {
            UseCsxWorkflow = true,
            TargetLanguageId = TargetLanguageId.C,
            FileExtension = ".plantuml", // use plantuml so that line endings aren't replaced for drawio
            PlantUmlDiagramTemplateId = TemplateIds.PlantUmlSimple1,
            smName = "LedSm"
        };

        var generator = new Generator(settings);
        var text = generator.GenerateDiagramFileText();

        text = StringUtils.ConvertToSlashNLines(text); // for below assertions
        text.Should().Contain("# transpilerId = \"C99\"");
    }
}
