using FluentAssertions;
using NSubstitute;
using StateSmith.Cli.Create;
using StateSmith.Cli.Utils;
using StateSmithTest;
using System.Text.RegularExpressions;
using Xunit;

namespace StateSmithCliTest.Create;

public class DrawioTests
{
    Settings settings = new Settings
    {
        UseCsxWorkflow = false,
        diagramFileName = "RocketSm.drawio.svg",
        FileExtension = ".drawio.svg",
        DrawIoDiagramTemplateId = TemplateIds.DrawIoPages1,
        smName = "RocketSm"
    };

    IFileWriter mockFileWriter = Substitute.For<IFileWriter>();

    /// <summary>
    /// I had an unreleased bug wrapping the XML inside an svg resulted in 6 mxfile tags instead of 2.
    /// </summary>
    [Fact]
    public void DrawIoPages1_SvgWrapper()
    {
        settings.TargetLanguageId = TargetLanguageId.C;
        settings.DrawIoDiagramTemplateId = TemplateIds.DrawIoPages1;
        Generator generator = new(settings);
        generator.tomlConfigType = TemplateLoader.TomlConfigType.Minimal;
        generator.SetFileWriter(mockFileWriter);

        // NSubsitute doesn't diff large strings very well, so we use ShouldBeShowDiff to show the differences
        mockFileWriter.When(x => x.Write("RocketSm.drawio.svg", Arg.Any<string>())).Do(x =>
        {
            var svg = x.ArgAt<string>(1);
            Regex.Matches(svg, @"\bmxfile\b").Count.Should().Be(2);
        });

        generator.GenerateFiles();

        // make sure the calls were made
        mockFileWriter.Received().Write("RocketSm.drawio.svg", Arg.Any<string>());
    }
}
