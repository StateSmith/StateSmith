using System.IO;
using Xunit;
using StateSmith.Input.DrawIo;
using FluentAssertions;
using System.Collections.Generic;
using System;
using StateSmith.Runner;

namespace StateSmithTest.DrawIo;

public class MxCellParserTests
{
    IConsolePrinter consolePrinter = new ConsolePrinter();

    [Fact]
    public void SanitizeLabelToNewLines()
    {
        void Test(string input)
        {
            string expected = "$NOTES\nblah";
            MxCellSanitizer.HtmlToPlainText(input, consolePrinter).Should().Be(expected); // unit test
            TestWithMxCell(input, expected);    // more of an integration test
        }

        Test(@"<div>$NOTES</div>blah");
        Test(@"$NOTES<div>blah</div>");
        Test(@"<p>$NOTES</p>blah");
        Test(@"$NOTES<p>blah</p>");

        Test(@"$NOTES</br>blah");
        Test(@"$NOTES<br>blah");
        Test(@"$NOTES<br/>blah");
        Test(@"$NOTES<br style=''>blah");
        Test(@"$NOTES<br style='color:blue;'>blah");
    }

    [Fact]
    public void TestFromEnemy3Sm()
    {
        TestWithMxCell(input: """NOTICE [noticeEvent.isGrenadeLive() &amp;&amp;&nbsp;<div>e.groundObject]</div>""",
                        expected: "NOTICE [noticeEvent.isGrenadeLive() && \ne.groundObject]");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/100
    /// </summary>
    [Fact]
    public void SanitizeLabelToSpaces()
    {
        void Test(char weirdSpaceChar)
        {
            string input = $"x{weirdSpaceChar}= y;";
            string expected = "x = y;";
            MxCellSanitizer.ProcessSpecialChars(input).Should().Be(expected); // unit test
            TestWithMxCell(input, expected);  // more of an integration test
        }

        // https://www.compart.com/en/unicode/category/Zs
        // https://en.wikipedia.org/wiki/Non-breaking_space#Encodings
        Test((char)0xA0); // NO-BREAK SPACE, &nbsp;
        Test((char)0x1680); // OGHAM SPACE MARK
        Test((char)0x2000); // EN QUAD
        Test((char)0x2001); // EM QUAD
        Test((char)0x2002); // EN SPACE
        Test((char)0x2003); // EM SPACE
        Test((char)0x2004); // THREE-PER-EM SPACE
        Test((char)0x2005); // FOUR-PER-EM SPACE
        Test((char)0x2006); // SIX-PER-EM SPACE
        Test((char)0x2007); // FIGURE SPACE
        Test((char)0x2008); // PUNCTUATION SPACE
        Test((char)0x2009); // THIN SPACE
        Test((char)0x200A); // HAIR SPACE
        Test((char)0x202F); // NARROW NO-BREAK SPACE
        Test((char)0x205F); // MEDIUM MATHEMATICAL SPACE
        Test((char)0x2060); // WORD JOINER
        Test((char)0x3000); // IDEOGRAPHIC SPACE
    }

    [Fact]
    public void Test()
    {
        string small1Xml = Small1Xml;
        MxCellParser mxCellParser = new(small1Xml, consolePrinter);
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

        smRoot.styleString.Should().Be("swimlane;fontStyle=1;align=left;spacingLeft=17;");
        smRoot.StyleMap.Should().BeEquivalentTo(new Dictionary<string,string> {
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

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/353
    /// </summary>
    [Fact]
    public void TestInvalidAttributeId_353()
    {
        const string small1Xml = """
            <mxfile host="65bd71144e">
                <diagram id="X5z84YncFC1j1vogRgUh" name="Page-1">
                    <mxGraphModel dx="901" dy="593" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="850" pageHeight="1100" math="0" shadow="0">
                        <root>
                            <mxCell id="0"/>
                            <mxCell />
                        </root>
                    </mxGraphModel>
                </diagram>
            </mxfile>
            """;
        MxCellParser mxCellParser = new(small1Xml, consolePrinter);
        Action a = () => mxCellParser.Parse();
        a.Should().Throw<Exception>()
            .WithMessage("*failed getting attribute `id`*")
            .WithMessage("*xml element `mxCell`*")
            .WithMessage("*line 6,*")
            .WithMessage("*column 18.*");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/353
    /// </summary>
    [Fact]
    public void TestInvalidAttributeId_Large_353()
    {
        MxCellParser mxCellParser = new(XmlWithMissingIdAttribute, consolePrinter);
        Action a = () => mxCellParser.Parse();
        a.Should().Throw<Exception>()
            .WithMessage("*failed getting attribute `id`*")
            .WithMessage("*xml element `mxCell`*")
            .WithMessage("*line 7,*")
            .WithMessage("*column 10.*");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/353
    /// </summary>
    [Fact]
    public void TestInvalidAttributeId_IntegrationTest_353()
    {
        var tempFileName = Path.GetTempFileName() + ".drawio";
        File.WriteAllText(tempFileName, XmlWithMissingIdAttribute);

        Action a = () =>
        {
            SmRunner smRunner = new(diagramPath: tempFileName);
            smRunner.Settings.propagateExceptions = true;
            smRunner.Run();
        };

        a.Should().Throw<Exception>()
            .WithMessage("*failed getting attribute `id`*")
            .WithMessage("*xml element `mxCell`*")
            .WithMessage("*line 5,*")
            .WithMessage("*relative to parent <mxGraphModel>*")
            .WithMessage("*column 10.*");

        File.Delete(tempFileName);
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

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/353
    /// </summary>
    private const string XmlWithMissingIdAttribute = """
        <mxfile host="Electron" modified="2023-04-24T17:11:48.107Z" agent="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) draw.io/21.1.2 Chrome/106.0.5249.199 Electron/21.4.3 Safari/537.36" etag="CukFah_wq8AoD-fPX12M" version="21.1.2" type="device">
          <diagram name="Page-1" id="etVsyBEr5ORxP3lcf0-8">
            <mxGraphModel dx="1434" dy="844" grid="0" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="0" pageScale="1" pageWidth="850" pageHeight="1100" math="0" shadow="0">
              <root>
                <mxCell id="0" />
                <mxCell id="1" parent="0" />
                <mxCell value="$STATEMACHINE : MarioSm" style="shape=swimlane;rotatable=0;align=center;verticalAlign=top;fontFamily=Lucida Console;startSize=30;fontSize=14;fontStyle=1;swimlaneFillColor=#18141d;rounded=1;arcSize=15;absoluteArcSize=1;fillColor=#1ba1e2;fontColor=#ffffff;strokeColor=#006EAF;" parent="1" vertex="1">
                  <mxGeometry x="10" y="70" width="940" height="710" as="geometry" />
                </mxCell>
              </root>
            </mxGraphModel>
          </diagram>
        </mxfile>
        """;

    private void TestWithMxCell(string input, string expected)
    {
        MxCell cell = GetHtmlCell();
        cell.label = input;
        MxCellSanitizer.SanitizeLabel(cell, consolePrinter);
        cell.label.Should().Be(expected);
    }

    private static MxCell GetHtmlCell()
    {
        MxCell cell = new("some_id");
        cell.SetStyle(MxCellSanitizer.StyleHtml, "1");
        return cell;
    }
}

