using FluentAssertions;
using StateSmith.Input.DrawIo;
using StateSmith.Output;
using StateSmith.Runner;
using StateSmith.SmGraph;
using StateSmithTest.Output;
using Xunit;

namespace StateSmithTest.Input.DrawIo;

public class MxCellsToSmDiagramConverterTests
{
    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/191
    /// </summary>
    [Fact]
    public void IsInnerHandlerText_Basic()
    {
        MxCell mxCell = MakeInnerHandlerTextCell();
        ExpectIsInnerText(mxCell);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/191
    /// </summary>
    [Fact]
    public void IsInnerHandlerText()
    {
        string[] requiredNoneStyles = {
            "fillColor",
            "gradientColor",
            "strokeColor",
        };

        foreach (var requiredStyle in requiredNoneStyles)
        {
            MxCell mxCell = MakeInnerHandlerTextCell();
            mxCell.SetStyle(requiredStyle, "");
            ExpectNotInnerText(mxCell);

            mxCell.SetStyle(requiredStyle, "blah");
            ExpectNotInnerText(mxCell);

            mxCell.SetStyle(requiredStyle, "none");
            ExpectIsInnerText(mxCell);
        }
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/191
    /// </summary>
    [Fact]
    public void IsInnerHandlerText_ShapeStyle()
    {
        MxCell mxCell = MakeInnerHandlerTextCell();
        mxCell.SetStyle("shape", "anything");
        ExpectNotInnerText(mxCell);

        mxCell.SetStyle("shape", "none");
        ExpectIsInnerText(mxCell);

        mxCell.SetStyle("shape", "");
        ExpectIsInnerText(mxCell);
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/191
    /// </summary>
    [Fact]
    public void IsInnerHandlerText_SpecialShapes()
    {
        foreach (var specialShape in MxCell.specialShapes)
        {
            MxCell mxCell = MakeInnerHandlerTextCell();
            mxCell.SetStyle(specialShape, "");
            ExpectNotInnerText(mxCell);

            mxCell.SetStyle(specialShape, "1");
            ExpectNotInnerText(mxCell);
        }
    }

    static void ExpectIsInnerText(MxCell mxCell)
    {
        MxCellsToSmDiagramConverter.IsInnerHandlerText(mxCell).Should().BeTrue();
    }

    static void ExpectNotInnerText(MxCell mxCell)
    {
        MxCellsToSmDiagramConverter.IsInnerHandlerText(mxCell).Should().BeFalse();
    }

    private static MxCell MakeInnerHandlerTextCell()
    {
        MxCell mxCell = new("some_id");
        mxCell.SetStyle("fillColor", "none");
        mxCell.SetStyle("gradientColor", "none");
        mxCell.SetStyle("strokeColor", "none");
        return mxCell;
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/192
    /// </summary>
    [Fact]
    public void Notes_IntegrationTest()
    {
        TestHelper.CaptureSmRunnerFiles(diagramPath: "NotesTest.drawio");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/192
    /// </summary>
    [Fact]
    public void Notes_CheckVertices()
    {
        InputSmBuilder builder = new();
        builder.ConvertDrawIoFileNodesToVertices(TestHelper.GetThisDir() + "NotesTest.drawio");
        builder.FindSingleStateMachine();

        var sm = builder.GetStateMachine();

        sm.DescendantsOfType<NotesVertex>().Count.Should().Be(6);
    }
}
