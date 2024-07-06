using FluentAssertions;
using StateSmith.Cli.Create;
using Xunit;

namespace StateSmithCliTest.Create;

public class SettingsTest
{
    [Fact]
    public void IsDiagramType()
    {
        var settings = new Settings();
        
        settings.FileExtension = ".drawio";
        settings.IsDrawIoSelected().Should().BeTrue();
        settings.IsDrawIoSvgSelected().Should().BeFalse();
        settings.IsPlantUmlSelected().Should().BeFalse();

        settings.FileExtension = ".drawio.svg";
        settings.IsDrawIoSelected().Should().BeTrue();
        settings.IsDrawIoSvgSelected().Should().BeTrue();
        settings.IsPlantUmlSelected().Should().BeFalse();

        settings.FileExtension = ".plantuml";
        settings.IsDrawIoSelected().Should().BeFalse();
        settings.IsDrawIoSvgSelected().Should().BeFalse();
        settings.IsPlantUmlSelected().Should().BeTrue();
    }
}
