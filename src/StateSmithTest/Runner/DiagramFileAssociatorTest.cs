using FluentAssertions;
using StateSmith.Runner;
using Xunit;

namespace StateSmithTest.Runner;

public class DiagramFileAssociatorTest
{
    [Fact]
    public void IsDrawIoFile_Match()
    {
        DiagramFileAssociator.IsDrawIoFile("test.drawio.svg").Should().BeTrue();
        DiagramFileAssociator.IsDrawIoFile("test.drawio").Should().BeTrue();
        DiagramFileAssociator.IsDrawIoFile("test.dio").Should().BeTrue();
        DiagramFileAssociator.IsDrawIoFile("test.drawio.SVG").Should().BeTrue();
        DiagramFileAssociator.IsDrawIoFile("test.drawIO").Should().BeTrue();
        DiagramFileAssociator.IsDrawIoFile("test.DIO").Should().BeTrue();
    }

    [Fact]
    public void IsDrawIoFile_NoMatch()
    {
        DiagramFileAssociator.IsDrawIoFile("test.svg").Should().BeFalse();
        DiagramFileAssociator.IsDrawIoFile("test.drawio.svg1").Should().BeFalse();
        DiagramFileAssociator.IsDrawIoFile("test.drawiosvg").Should().BeFalse();
    }
}
