using System.IO;
using Xunit;
using StateSmith.Input.DrawIo;
using FluentAssertions;
using System.Collections.Generic;

namespace StateSmithTest.DrawIo;

public class MxCellParserTests
{
    [Fact]
    public void SanitizeLabelToNewLines()
    {
        static void Test(string input)
        {
            string expected = "$NOTES\nblah";
            MxCellSanitizer.SanitizeLabelHtml(input).Should().Be(expected); // unit test
            TestWithMxCell(input, expected);    // more of an integration test
        }

        Test(@"$NOTES</div>blah");
        Test(@"$NOTES</p>blah");

        Test(@"$NOTES</br>blah");
        Test(@"$NOTES<br>blah");
        Test(@"$NOTES<br/>blah");
        Test(@"$NOTES<br style=''>blah");
        Test(@"$NOTES<br style='color:blue;'>blah");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/100
    /// </summary>
    [Fact]
    public void SanitizeLabelToSpaces()
    {
        static void Test(char weirdSpaceChar)
        {
            string input = $"x{weirdSpaceChar}= y;";
            string expected = "x = y;";
            MxCellSanitizer.ProcessSpecialChars(input).Should().Be(expected); // unit test
            TestWithMxCell(input, expected);  // more of an integration test
        }

        // https://www.compart.com/en/unicode/category/Zs
        // https://en.wikipedia.org/wiki/Non-breaking_space#Encodings
        Test((char)0xA0); // NO-BREAK SPACE, &nbsp;
        Test((char)0x2007); // FIGURE SPACE
        Test((char)0x202F); // NARROW NO-BREAK SPACE
        Test((char)0x2060); // WORD JOINER

        // TODO - update for https://github.com/StateSmith/StateSmith/issues/100
    }

    [Fact]
    public void Test()
    {
        string small1Xml = Small1Xml;
        MxCellParser mxCellParser = new(new StringReader(small1Xml));
        mxCellParser.Parse();

        var smRoot = mxCellParser.GetCellById("5hg7lKXR2ijf20l0Po2r-1");
        smRoot.label.Should().Be("$STATEMACHINE : Small1");
        smRoot.type.Should().Be(MxCell.Type.Vertex);
        smRoot.parent.Should().Be("1");
        smRoot.source.Should().BeNull();
        smRoot.target.Should().BeNull();
        smRoot.isCollapsed.Should().BeTrue();
        smRoot.primaryBounds.Should().BeEquivalentTo(new MxBounds { x = 20, y = 30, width = 200, height = 90 });
        smRoot.alternateBounds.Should().BeEquivalentTo(new MxBounds { x = 20, y = 30, width = 430.7, height = 510 });

        smRoot.style.Should().Be("swimlane;fontStyle=1;align=left;spacingLeft=17;");
        smRoot.styleMap.Should().BeEquivalentTo(new Dictionary<string,string> {
            {"swimlane", ""},
            {"fontStyle", "1"},
            {"align", "left"},
            {"spacingLeft", "17"},
        });

        var edge = mxCellParser.GetCellById("5hg7lKXR2ijf20l0Po2r-11");
        edge.label.Should().Be("INC_123");
        edge.type.Should().Be(MxCell.Type.Edge);
        edge.parent.Should().Be("5hg7lKXR2ijf20l0Po2r-1");
        edge.source.Should().Be("5hg7lKXR2ijf20l0Po2r-12");
        edge.target.Should().Be("5hg7lKXR2ijf20l0Po2r-8");

        var onGroup = mxCellParser.GetCellById("5hg7lKXR2ijf20l0Po2r-6");
        onGroup.isCollapsed.Should().BeFalse();
        //<mxGeometry x="65" y="225" width="305" height="245" as="geometry">
        //<mxRectangle x="65" y="225" width="200" height="90" as="alternateBounds"/>
        onGroup.primaryBounds.Should().BeEquivalentTo(new MxBounds { x = 65, y = 225, width = 305, height = 245});
        onGroup.alternateBounds.Should().BeEquivalentTo(new MxBounds { x = 65, y = 225, width = 200, height = 90});

        //could test all vertices and edges...
    }



    private const string Small1Xml = """
            <mxfile host="65bd71144e">
                <diagram id="X5z84YncFC1j1vogRgUh" name="Page-1">
                    <mxGraphModel dx="901" dy="593" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="850" pageHeight="1100" math="0" shadow="0">
                        <root>
                            <mxCell id="0"/>
                            <mxCell id="1" parent="0"/>
                            <mxCell id="5hg7lKXR2ijf20l0Po2r-1" value="$STATEMACHINE : Small1" style="swimlane;fontStyle=1;align=left;spacingLeft=17;" parent="1" vertex="1" collapsed="1">
                                <mxGeometry x="20" y="30" width="200" height="90" as="geometry">
                                    <mxRectangle x="20" y="30" width="430.7" height="510" as="alternateBounds"/>
                                </mxGeometry>
                            </mxCell>
                            <mxCell id="5hg7lKXR2ijf20l0Po2r-6" value="ON_GROUP" style="swimlane;fontStyle=1;align=left;spacingLeft=17;" parent="5hg7lKXR2ijf20l0Po2r-1" vertex="1">
                                <mxGeometry x="65" y="225" width="305" height="245" as="geometry">
                                    <mxRectangle x="65" y="225" width="200" height="90" as="alternateBounds"/>
                                </mxGeometry>
                            </mxCell>
                            <mxCell id="5hg7lKXR2ijf20l0Po2r-7" value="INC" style="rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=0.5;exitY=1;exitDx=0;exitDy=0;entryX=0.5;entryY=0;entryDx=0;entryDy=0;fontColor=default;" parent="5hg7lKXR2ijf20l0Po2r-6" source="5hg7lKXR2ijf20l0Po2r-8" target="5hg7lKXR2ijf20l0Po2r-10" edge="1">
                                <mxGeometry relative="1" as="geometry"/>
                            </mxCell>
                            <mxCell id="5hg7lKXR2ijf20l0Po2r-8" value="&lt;div&gt;&lt;b&gt;ON1&lt;/b&gt;&lt;/div&gt;&lt;div&gt;enter / light_on1();&lt;/div&gt;" style="rounded=1;whiteSpace=wrap;html=1;verticalAlign=top;align=left;spacingLeft=4;" parent="5hg7lKXR2ijf20l0Po2r-6" vertex="1">
                                <mxGeometry x="50" y="50" width="200" height="60" as="geometry"/>
                            </mxCell>
                            <mxCell id="5hg7lKXR2ijf20l0Po2r-9" value="DIM" style="edgeStyle=orthogonalEdgeStyle;curved=1;rounded=0;orthogonalLoop=1;jettySize=auto;exitX=0.75;exitY=0;exitDx=0;exitDy=0;entryX=0.75;entryY=1;entryDx=0;entryDy=0;fontColor=default;" parent="5hg7lKXR2ijf20l0Po2r-6" source="5hg7lKXR2ijf20l0Po2r-10" target="5hg7lKXR2ijf20l0Po2r-8" edge="1">
                                <mxGeometry relative="1" as="geometry"/>
                            </mxCell>
                            <mxCell id="5hg7lKXR2ijf20l0Po2r-10" value="&lt;div&gt;&lt;span&gt;ON2&lt;/span&gt;&lt;/div&gt;&lt;div&gt;&lt;span&gt;enter / light_on2();&lt;/span&gt;&lt;/div&gt;" style="rounded=1;whiteSpace=wrap;html=1;verticalAlign=top;align=left;spacingLeft=4;fontStyle=0" parent="5hg7lKXR2ijf20l0Po2r-6" vertex="1">
                                <mxGeometry x="50" y="170" width="200" height="60" as="geometry"/>
                            </mxCell>
                            <mxCell id="5hg7lKXR2ijf20l0Po2r-11" value="INC_123" style="rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.5;entryY=0;entryDx=0;entryDy=0;fontColor=default;" parent="5hg7lKXR2ijf20l0Po2r-1" source="5hg7lKXR2ijf20l0Po2r-12" target="5hg7lKXR2ijf20l0Po2r-8" edge="1">
                                <mxGeometry x="-0.5" relative="1" as="geometry">
                                    <mxPoint as="offset"/>
                                </mxGeometry>
                            </mxCell>
                            <mxCell id="5hg7lKXR2ijf20l0Po2r-12" value="&lt;div&gt;&lt;b&gt;OFF&lt;/b&gt;&lt;/div&gt;&lt;div&gt;enter / light_off();&lt;/div&gt;" style="rounded=1;whiteSpace=wrap;html=1;verticalAlign=top;align=left;spacingLeft=4;" parent="5hg7lKXR2ijf20l0Po2r-1" vertex="1">
                                <mxGeometry x="115" y="110" width="200" height="60" as="geometry"/>
                            </mxCell>
                            <mxCell id="5hg7lKXR2ijf20l0Po2r-13" value="" style="rounded=0;comic=0;orthogonalLoop=1;jettySize=auto;html=0;fontColor=default;" parent="5hg7lKXR2ijf20l0Po2r-1" source="5hg7lKXR2ijf20l0Po2r-14" target="5hg7lKXR2ijf20l0Po2r-12" edge="1">
                                <mxGeometry relative="1" as="geometry"/>
                            </mxCell>
                            <mxCell id="5hg7lKXR2ijf20l0Po2r-14" value="$initial" style="ellipse;whiteSpace=wrap;html=1;fillColor=#000000;fontColor=none;noLabel=1;" parent="5hg7lKXR2ijf20l0Po2r-1" vertex="1">
                                <mxGeometry x="202.5" y="50" width="30" height="30" as="geometry"/>
                            </mxCell>
                            <mxCell id="5hg7lKXR2ijf20l0Po2r-15" value="DIM" style="rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=0.75;exitY=0;exitDx=0;exitDy=0;entryX=0.75;entryY=1;entryDx=0;entryDy=0;fontColor=default;" parent="5hg7lKXR2ijf20l0Po2r-1" source="5hg7lKXR2ijf20l0Po2r-8" target="5hg7lKXR2ijf20l0Po2r-12" edge="1">
                                <mxGeometry x="-0.7" relative="1" as="geometry">
                                    <mxPoint as="offset"/>
                                </mxGeometry>
                            </mxCell>
                        </root>
                    </mxGraphModel>
                </diagram>
            </mxfile>
            """;

    private static void TestWithMxCell(string input, string expected)
    {
        MxCell cell = GetHtmlCell();
        cell.label = input;
        MxCellSanitizer.SanitizeLabel(cell);
        cell.label.Should().Be(expected);
    }

    private static MxCell GetHtmlCell()
    {
        MxCell cell = new("some_id");
        cell.styleMap[MxCellSanitizer.StyleHtml] = "1";
        return cell;
    }
}

