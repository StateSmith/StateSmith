using FluentAssertions;
using StateSmith.Cli.Run;
using Xunit;

namespace StateSmith.CliTest.Run;

public class SsDiagramFilterTest
{
    readonly SsDiagramFilter diagramFilter = new(new());

    private const string PlantUmlMatch = """
            @startuml RocketController
            [*] --> State1
            @enduml
            """;

    private const string PlantUmlMatchFileName = """
            @startuml {fileName}
            [*] --> State1
            @enduml
            """;

    /// <summary>
    /// Missing diagram name.
    /// </summary>
    private const string PlantUmlNoMatch = """
            @startuml
            [*] --> State1
            @enduml
            """;
    private const string DrawioYedMatch = "$STATEMACHINE";
    private const string DrawioYedNoMatch = "$Statemachine";

    [Fact]
    public void IsTargetDiagramFile_Match()
    {
        diagramFilter.IsTargetDiagramFile("blah.plantuml", PlantUmlMatch).Should().BeTrue();
        diagramFilter.IsTargetDiagramFile("blah.drawio", DrawioYedMatch).Should().BeTrue();
    }

    [Fact]
    public void IsTargetDiagramFile_NoMatch()
    {
        diagramFilter.IsTargetDiagramFile("blah.plantuml", PlantUmlNoMatch).Should().BeFalse();
        diagramFilter.IsTargetDiagramFile("blah.drawio", DrawioYedNoMatch).Should().BeFalse();
        diagramFilter.IsTargetDiagramFile("blah.drawio", PlantUmlMatch).Should().BeFalse("wrong contents for file type");
        diagramFilter.IsTargetDiagramFile("blah.plantuml", DrawioYedMatch).Should().BeFalse("wrong contents for file type");
    }

    [Fact]
    public void IsSsDrawioYedFileContents()
    {
        diagramFilter.IsSsDrawioYedFileContents(DrawioYedMatch).Should().BeTrue();
        diagramFilter.IsSsDrawioYedFileContents(DrawioYedNoMatch).Should().BeFalse();
    }

    [Fact]
    public void IsSsPlantUmlFileContents()
    {
        diagramFilter.IsSsPlantUmlFileContents(PlantUmlMatch).Should().BeTrue();
        diagramFilter.IsSsPlantUmlFileContents(PlantUmlMatchFileName).Should().BeTrue();
        diagramFilter.IsSsPlantUmlFileContents(PlantUmlNoMatch).Should().BeFalse();
    }
}
