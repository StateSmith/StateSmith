using FluentAssertions;
using NSubstitute;
using StateSmithCli.Create;
using StateSmithCli.Utils;
using StateSmithTest;
using Xunit;

namespace StateSmithCliTest.Create;

public class GeneratorTest
{
    [Fact]
    public void IntegrationTest()
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
            DrawIoDiagramTemplateId = "test-drawio-1",
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
}
