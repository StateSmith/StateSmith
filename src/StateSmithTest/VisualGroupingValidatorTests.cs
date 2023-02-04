using Xunit;
using StateSmith.Input.DrawIo;
using System;
using FluentAssertions;
using StateSmith.SmGraph;
using System.Linq;
using StateSmith.Runner;

namespace StateSmithTest.DrawIo;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/81
/// </summary>
public class VisualGroupingValidatorTests
{
    readonly InputSmBuilder runner = new();
    DrawIoToSmDiagramConverter converter;

    public VisualGroupingValidatorTests()
    {
        converter = runner.ssServiceProvider.GetServiceOrCreateInstance();
    }

    [Fact]
    public void Ok1()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/visual-group-detection/ok1.drawio";
        converter.ProcessFile(filePath); // shouldn't throw
    }

    [Fact]
    public void LooksNotGrouped_Left()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/visual-group-detection/looks_not_grouped_left.drawio";
        AssertThrows(filePath, "left");
    }

    [Fact]
    public void LooksNotGrouped_IgnoredSetting()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/visual-group-detection/looks_not_grouped_left.drawio";
        GetDrawIoSettings().checkForChildStateContainment = false;
        converter.ProcessFile(filePath); // would normally throw
    }

    private DrawIoSettings GetDrawIoSettings()
    {
        return runner.ssServiceProvider.GetServiceOrCreateInstance();
    }

    [Fact]
    public void LooksNotGrouped_Above()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/visual-group-detection/looks_not_grouped_above.drawio";
        AssertThrows(filePath, "above");
    }

    [Fact]
    public void LooksNotGrouped_Right()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/visual-group-detection/looks_not_grouped_right.drawio";
        AssertThrows(filePath, "right");
    }

    [Fact]
    public void LooksNotGrouped_Below()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/visual-group-detection/looks_not_grouped_below.drawio";
        AssertThrows(filePath, "below");
    }

    [Fact]
    public void LooksGrouped1()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/visual-group-detection/looks_grouped.drawio";
        Action action = () => converter.ProcessFile(filePath);
        action.Should().Throw<DiagramNodeException>().Where(e => e.Node.label.StartsWith("NOT_IN_GROUP")).WithMessage("*overlap*NOT_IN_GROUP*").WithMessage("*overlap*$STATEMACHINE*");
    }

    [Fact]
    public void LooksGrouped_IgnoredSetting()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/visual-group-detection/looks_grouped.drawio";
        GetDrawIoSettings().checkForBadOverlap = false;
        converter.ProcessFile(filePath); // would normally throw
    }

    private void AssertThrows(string filePath, string expectedMessageEnd)
    {
        Action action = () => converter.ProcessFile(filePath);
        action.Should().Throw<DiagramNodeException>().Where(e => e.Node.label.StartsWith("ACTUALLY_IN_GROUP\n")).WithMessage($"*outside of parent grouping*{expectedMessageEnd}*");
    }
}

