using Xunit;
using StateSmith.Input.DrawIo;
using System;
using FluentAssertions;
using StateSmith.SmGraph;
using System.Linq;
using StateSmith.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace StateSmithTest.DrawIo.Issue81;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/81
/// </summary>
public class VisualGroupingValidatorTests
{
    readonly InputSmBuilder runner = TestHelper.CreateServiceProvider().GetRequiredService<InputSmBuilder>();
    DrawIoToSmDiagramConverter converter;

    public VisualGroupingValidatorTests()
    {
        converter = runner.sp.GetRequiredService<DrawIoToSmDiagramConverter>();
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
        AssertThrowsForActuallyInGroup(filePath, "left");
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
        return runner.sp.GetRequiredService<DrawIoSettings>();
    }

    [Fact]
    public void LooksNotGrouped_Above()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/visual-group-detection/looks_not_grouped_above.drawio";
        AssertThrowsForActuallyInGroup(filePath, "above");
    }

    [Fact]
    public void LooksNotGrouped_Right()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/visual-group-detection/looks_not_grouped_right.drawio";
        AssertThrowsForActuallyInGroup(filePath, "right");
    }

    [Fact]
    public void LooksNotGrouped_Below()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/visual-group-detection/looks_not_grouped_below.drawio";
        AssertThrowsForActuallyInGroup(filePath, "below");
    }

    [Fact]
    public void LooksGrouped1()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/visual-group-detection/looks_grouped.drawio";
        Action action = () => converter.ProcessFile(filePath);
        action.Should().Throw<DiagramNodeException>().Where(e => e.Node.label.StartsWith("NOT_IN_GROUP")).WithMessage("*overlap*NOT_IN_GROUP*").WithMessage("*overlap*$STATEMACHINE*").WithMessage("* https://github.com/StateSmith/StateSmith/issues/81 *");
    }

    [Fact]
    public void LooksGrouped_IgnoredSetting()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/visual-group-detection/looks_grouped.drawio";
        GetDrawIoSettings().checkForBadOverlap = false;
        converter.ProcessFile(filePath); // would normally throw
    }

    private void AssertThrowsForActuallyInGroup(string filePath, string expectedMessageEnd)
    {
        Action action = () => converter.ProcessFile(filePath);
        action.Should().Throw<DiagramNodeException>().Where(e => e.Node.label.StartsWith("ACTUALLY_IN_GROUP\n")).WithMessage($"*outside of parent grouping*{expectedMessageEnd}*").WithMessage("* https://github.com/StateSmith/StateSmith/issues/81 *");
    }
}

