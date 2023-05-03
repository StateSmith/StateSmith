using FluentAssertions;
using StateSmith.Input.DrawIo;
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
}
