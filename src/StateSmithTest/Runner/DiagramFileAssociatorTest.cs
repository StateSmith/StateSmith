using FluentAssertions;
using StateSmith.Runner;
using Xunit;

namespace StateSmithTest.Runner;

public class DiagramFileAssociatorTest
{
    readonly DiagramFileAssociator diagramFileAssociator = new();

    [Fact]
    public void IsDrawIoFile_Match()
    {
        diagramFileAssociator.IsDrawIoFile("test.drawio.svg").Should().BeTrue();
        diagramFileAssociator.IsDrawIoFile("test.drawio").Should().BeTrue();
        diagramFileAssociator.IsDrawIoFile("test.dio").Should().BeTrue();
        diagramFileAssociator.IsDrawIoFile("test.drawio.SVG").Should().BeTrue();
        diagramFileAssociator.IsDrawIoFile("test.drawIO").Should().BeTrue();
        diagramFileAssociator.IsDrawIoFile("test.DIO").Should().BeTrue();
    }

    [Fact]
    public void IsDrawIoFile_NoMatch()
    {
        diagramFileAssociator.IsDrawIoFile("test.svg").Should().BeFalse();
        diagramFileAssociator.IsDrawIoFile("test.drawio.svg1").Should().BeFalse();
        diagramFileAssociator.IsDrawIoFile("test.drawiosvg").Should().BeFalse();
    }
}
