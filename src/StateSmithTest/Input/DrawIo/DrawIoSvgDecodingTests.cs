using System;
using System.IO;
using Xunit;
using StateSmith.Input.DrawIo;
using FluentAssertions;
using System.Linq;

namespace StateSmithTest.Input.DrawIo;

public class DrawIoSvgDecodingTests
{
    [Fact]
    public void Decompress()
    {
        string input = "zVZdb5swFP01eWzER0iy16RdqmmVtqbSpr1UFlyMFWMjYwrZr5+NbQglZG2lSOUFfO69/jjnXItZuM2bnUBF9sAToLPAS5pZeDsLAt+LPPXSyNEg0So0ABYksUk9sCd/wVVatCIJlINEyTmVpBiCMWcMYjnAkBC8HqalnA5XLRCGEbCPER2jv0giM4Ou3bE0fg8EZ25l37ORHLlkC5QZSnh9AoV3s3ArOJfmK2+2QDV5jhdT93Ui2m1MAJNvKWB1svv5Bx98+u1Z/v7xhFmzv7GzvCBa2QPbzcqjY0Dtu9CfeYO1xPOcx4eqmCvCJSIMRDnPkcCEPbb0b4R6PbTAEy9UmRIy3JRS8ANsOeWinTRctk8XcdT6CklQmYHesy4UvGJJO9IhJGLrkMisVImSvMAjlAbVFfZAICQ0k0z5Hf/KuMBzkOKoUppORFNiPesv7LjuHRA4nbMT9QOXiKzrcDd3L4z6sNq8Q6dgpNNOUVNMiiWMGEPeGWcqtkkJpSdSeN46TtOPSxHoYKrccDJn2j4Wt3n+Sq9RoJgwrMv6kTHKjUYQJZipAYVU7x9Vkltt/T7/exu8XbQe0GG3Z69Dup7UUAGCKBnghIM3uWS6YyatE42M8uWMT8Jr2ST6fzsPZawzImGvaNXRWvW3wjKZUxu+Fk1+NGiwaNxffnCGt+W1eAtHvG3d9TZusZrkFF12UcwpRUUJ03f1ZX4W61c30PIMQ+duoEvOsqvpexoxrI7zruXOX3jeq/UQVW3GkISNtlk5EqY76se1Wnwaj4eXNRxyurqex9Ww/40wNPc/Y+HdPw==";
        var expected = """
            <mxGraphModel dx="1050" dy="573" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="850" pageHeight="1100" math="0" shadow="0"><root><mxCell id="0"/><mxCell id="1" parent="0"/><mxCell id="nwdGQZgk1lJ_tXPTgnxS-1" value="" style="shape=mxgraph.mockup.containers.marginRect;rectMarginTop=10;strokeColor=#666666;strokeWidth=1;dashed=0;rounded=1;arcSize=5;recursiveResize=0;" vertex="1" parent="1"><mxGeometry x="110" y="140" width="200" height="240" as="geometry"/></mxCell><mxCell id="nwdGQZgk1lJ_tXPTgnxS-2" value="Group" style="shape=rect;strokeColor=none;fillColor=#008cff;strokeWidth=1;dashed=0;rounded=1;arcSize=20;fontColor=#ffffff;fontSize=17;spacing=2;spacingTop=-2;align=left;autosize=1;spacingLeft=4;resizeWidth=0;resizeHeight=0;perimeter=none;" vertex="1" parent="nwdGQZgk1lJ_tXPTgnxS-1"><mxGeometry x="5" width="90" height="30" as="geometry"/></mxCell><mxCell id="nwdGQZgk1lJ_tXPTgnxS-5" value="" style="rounded=1;whiteSpace=wrap;html=1;" vertex="1" parent="nwdGQZgk1lJ_tXPTgnxS-1"><mxGeometry x="15" y="50" width="120" height="60" as="geometry"/></mxCell><mxCell id="nwdGQZgk1lJ_tXPTgnxS-3" value="Container" style="swimlane;" vertex="1" collapsed="1" parent="1"><mxGeometry x="480" y="160" width="100" height="30" as="geometry"><mxRectangle x="480" y="160" width="200" height="200" as="alternateBounds"/></mxGeometry></mxCell><mxCell id="nwdGQZgk1lJ_tXPTgnxS-4" value="" style="rounded=1;whiteSpace=wrap;html=1;" vertex="1" parent="nwdGQZgk1lJ_tXPTgnxS-3"><mxGeometry x="40" y="70" width="120" height="60" as="geometry"/></mxCell></root></mxGraphModel>
            """;

        var actual = DrawIoDecoder.DecompressContent(input);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetDiagramCompressedContents_Test()
    {
        var contents = GetDiagramsContentsRaw_First("""<mxfile><diagram id="Tqm6eFcu1KHT34LG2WWE" name="Page-1">blah123</diagram></mxfile>""");
        Assert.Equal("blah123", contents);
    }

    [Fact]
    public void GetDiagramCompressedContents_WhitespaceOk()
    {
        var contents = GetDiagramsContentsRaw_First("""  <mxfile>  <diagram id="Tqm6eFcu1KHT34LG2WWE" name="Page-1">blah123</diagram>  </mxfile>   """);
        Assert.Equal("blah123", contents);
    }

    [Fact]
    public void GetDiagramContents_WhitespaceOk2()
    {
        var contents = GetDiagramsContentsRaw_First("""
            <mxfile>
                <diagram id="Tqm6eFcu1KHT34LG2WWE" name="Page-1">
                    blah123
                </diagram>
            </mxfile>
            """);
        Assert.Equal("blah123", contents);
    }

    [Fact]
    public void GetDiagramCompressedContents_BadMxfileOpening()
    {
        Action action = () => GetDiagramsContentsRaw_First("""<Mxfile><diagram id="Tqm6eFcu1KHT34LG2WWE" name="Page-1">blah123</diagram></mxfile>""");
        action.Should().Throw<DrawIoException>().WithMessage("*opening xml tag `mxfile`*found `Mxfile`*");
    }

    [Fact]
    public void GetDiagramCompressedContents_BadDiagramOpening()
    {
        Action action = () => GetDiagramsContentsRaw_First("""<mxfile><SomeDiagram id="Tqm6eFcu1KHT34LG2WWE" name="Page-1">blah123</diagram></mxfile>""");
        action.Should().Throw<DrawIoException>().WithMessage("*opening xml tag `diagram`*found `SomeDiagram`*");
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/78
    /// </summary>
    [Fact]
    public void GetDiagramContents_AdditionalDiagramAllowed()
    {
        DrawIoDecoder.GetDiagramsContentsRaw("""
            <mxfile>
                <diagram id="123" name="Page-1">blah123</diagram>
                <diagram id="123" name="Page-2">blah123</diagram>
            </mxfile>
            """);
    }

    [Fact]
    public void GetDiagramContents_BadClosing()
    {
        Action action = () => DrawIoDecoder.GetDiagramsContentsRaw("""
            <mxfile>
                <diagram id="123" name="Page-1">blah123</diagram>
                <b>stuff</b>
            </mxfile>
            """);
        action.Should().Throw<DrawIoException>().WithMessage("*closing xml tag `mxfile`*found `b`*");
    }

    [Fact]
    public void DecodeToOriginalDiagram_StaticString()
    {
        string svgFileStart = """
            <svg host="65bd71144e" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" version="1.1" width="491px" height="791px" viewBox="-0.5 -0.5 491 791" content="&lt;mxfile&gt;&lt;diagram id=&quot;Tqm6eFcu1KHT34LG2WWE&quot; name=&quot;Page-1&quot;&gt;7Vpbb6s4EP41SLsP5wgDCeljbr2s2uSo6dHZPlUOOOCVwRE4t/76NWAHiGkCTdOkUquowuPL2J5vvhkm0cx+sL6J4Nx/oC4imqG7a80caIZxdaXz/4lgkwnawMwEXoTdTARywQS/IiEU87wFdlFcGsgoJQzPy0KHhiFyWEkGo4iuysNmlJS1zqGHFMHEgUSV/sEu8zNpp6Xn8luEPV9qBrroCaAcLASxD126KojMoWb2I0pZ9hSs+4gkdyfvJZt3/UbvdmMRClmdCc+b9eof3Rssr29AdP/iv77+uf0BrrJllpAsxIm1vqF1+VK68mBNnrpPw4du//ZuxFV0edfTgtEIQwImgTgl28iri1c4IDDkrd6MhmwiegBvQ4K9kD8TNON778Vz6ODQu09bA2BzkXo6ceAlihhaF0TitDeIBohFGz5E9Jri4jdlRK1yM1oSnX7BhLYUQgEdb7vyVtkjhxoMPX6erTZwUNt2Q0VtW7xIbZAwFIWQoR5dhG5cNCl/KJwzF6WGbmB0w1aMPh693DyOf/86tRH3g7C2advlyzaMlnLbZtVtW+Bdtm23yupM1bjAqlJ3Btt2FNvejfqKWaNkAyhZSOd2ohHzqUdDSO4pnQvz/ocY2whChtzNuchnARG9aI3Zv8n0ny3Rei70DNZi5bSxkY2Qn60wKWk+F/vyaWlLzkuA16eERrztohlcEFYfXBLsMV1EDto3UKCQwchD+1Y0jWwgckuhQwVrhAhkeFmOJFXIK4GgscUrKNxop1fk4iV/9LLbykRTKRiPgJRxtdPdcVxWMbsg4reOoiQ48M0bOklA/0JD8NffexZ5E4QJblY+ZmjCSSTpXfFkogy4hBwwj8tdwTosAep+DrKao6Q+BbXKFNSq4Hu9ghLaewjoKBhITjrg+AloJYnnbj/MpT1nES23RjmeJy7a56UrH/Z566J83gSKsQd3D+cxdh4J7GIo2LL/oVBgl3ABvhYuJPleCi6MJrGAs2WYhwOjwNrFntoRoWrZ6ihh7EaJOvrOEDyKGa/eEGPvjiTAPnsoMb/ZpcguDg2wI0Z8ONNYXzPrlPvezzTjkfnOhFKzGxEPDBKHDqfxPG3rDgcbS5ZJdqbrDWlsdzXJW+be7FZdyR7UHgt+cj08YXscdifD7S2IY/SSzzczGp2azAj2veYfB/vWd5a9W7Y5zHHtj86mBD5+JMdNZ9QmPbHWL4pTfpBD6GwW862doCAjD9/09fz6+uNfz2ezL/p63rxCCHbrsUAtEX5yVqVWXT+WN4ppSkMO+UQu6NTNd9qXle+odVXNsHCIGYZEtSMheB6jw24zw4TIS9cMU0//dowR0rT8HtJ7OEVy4ukc56pc7G5VlNYr3MY8mduo5c2zvYxU1r8vvupR2zfrV8Dr+mYep+1Lj9OWWlK7hMzuk1i57luo/EbrQljZUoveFblQbzx+OCpxmlIaXFrm9J43sOPTqLZ97jSqU5VGWY5PMb9fxRA+DaaLuDqgvhV7PynQmjtfYp8u0PJm/jOTjC/z3+qYw/8B&lt;/diagram&gt;&lt;/mxfile&gt;">
                <defs/>
            """;

        string expected = """
            <mxGraphModel dx="990" dy="613" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="850" pageHeight="1100" math="0" shadow="0"><root><mxCell id="0"/><mxCell id="1" parent="0"/><mxCell id="YyxwJ0gDvFG1rL_hzzWH-19" value="     $STATEMACHINE: Tutorial1Sm" style="swimlane;fontStyle=1;align=left;spacingLeft=17;" parent="1" vertex="1"><mxGeometry x="30" y="10" width="490" height="790" as="geometry"><mxRectangle x="10" y="10" width="230" height="100" as="alternateBounds"/></mxGeometry></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-27" value="ON_GROUP" style="swimlane;fontStyle=1;align=left;spacingLeft=17;" parent="YyxwJ0gDvFG1rL_hzzWH-19" vertex="1"><mxGeometry x="60" y="225" width="330" height="410" as="geometry"><mxRectangle x="65" y="230" width="140" height="40" as="alternateBounds"/></mxGeometry></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-28" value="INC" style="rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=0.5;exitY=1;exitDx=0;exitDy=0;entryX=0.5;entryY=0;entryDx=0;entryDy=0;fontColor=default;" parent="YyxwJ0gDvFG1rL_hzzWH-27" source="YyxwJ0gDvFG1rL_hzzWH-29" target="YyxwJ0gDvFG1rL_hzzWH-32" edge="1"><mxGeometry relative="1" as="geometry"/></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-29" value="&lt;div&gt;&lt;b&gt;ON1&lt;/b&gt;&lt;/div&gt;&lt;div&gt;enter / light_on1();&lt;/div&gt;" style="rounded=1;whiteSpace=wrap;html=1;verticalAlign=top;align=left;spacingLeft=4;" parent="YyxwJ0gDvFG1rL_hzzWH-27" vertex="1"><mxGeometry x="50" y="50" width="200" height="60" as="geometry"/></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-30" value="INC" style="edgeStyle=orthogonalEdgeStyle;curved=1;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.5;entryY=0;entryDx=0;entryDy=0;fontColor=default;" parent="YyxwJ0gDvFG1rL_hzzWH-27" source="YyxwJ0gDvFG1rL_hzzWH-32" target="YyxwJ0gDvFG1rL_hzzWH-34" edge="1"><mxGeometry relative="1" as="geometry"/></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-31" value="DIM" style="edgeStyle=orthogonalEdgeStyle;curved=1;rounded=0;orthogonalLoop=1;jettySize=auto;exitX=0.75;exitY=0;exitDx=0;exitDy=0;entryX=0.75;entryY=1;entryDx=0;entryDy=0;fontColor=default;" parent="YyxwJ0gDvFG1rL_hzzWH-27" source="YyxwJ0gDvFG1rL_hzzWH-32" target="YyxwJ0gDvFG1rL_hzzWH-29" edge="1"><mxGeometry relative="1" as="geometry"/></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-32" value="&lt;div&gt;&lt;span&gt;ON2&lt;/span&gt;&lt;/div&gt;&lt;div&gt;&lt;span&gt;enter / light_on2();&lt;/span&gt;&lt;/div&gt;" style="rounded=1;whiteSpace=wrap;html=1;verticalAlign=top;align=left;spacingLeft=4;fontStyle=0" parent="YyxwJ0gDvFG1rL_hzzWH-27" vertex="1"><mxGeometry x="50" y="170" width="200" height="60" as="geometry"/></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-33" value="DIM" style="edgeStyle=orthogonalEdgeStyle;curved=1;rounded=0;orthogonalLoop=1;jettySize=auto;exitX=0.75;exitY=0;exitDx=0;exitDy=0;entryX=0.75;entryY=1;entryDx=0;entryDy=0;fontColor=default;comic=0;" parent="YyxwJ0gDvFG1rL_hzzWH-27" source="YyxwJ0gDvFG1rL_hzzWH-34" target="YyxwJ0gDvFG1rL_hzzWH-32" edge="1"><mxGeometry relative="1" as="geometry"/></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-34" value="&lt;div&gt;ON3&lt;/div&gt;&lt;div&gt;enter / {&lt;/div&gt;&lt;div&gt;&amp;nbsp; count = 0;&lt;/div&gt;&lt;div&gt;&amp;nbsp; light_on3();&lt;/div&gt;&lt;div&gt;}&lt;/div&gt;&lt;div&gt;1. INCREASE / count++;&lt;/div&gt;" style="rounded=1;whiteSpace=wrap;html=1;verticalAlign=top;align=left;spacingLeft=4;fontStyle=0" parent="YyxwJ0gDvFG1rL_hzzWH-27" vertex="1"><mxGeometry x="50" y="280" width="200" height="110" as="geometry"/></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-35" value="INC" style="edgeStyle=orthogonalEdgeStyle;curved=1;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.5;entryY=0;entryDx=0;entryDy=0;fontColor=default;" parent="YyxwJ0gDvFG1rL_hzzWH-19" source="YyxwJ0gDvFG1rL_hzzWH-36" target="YyxwJ0gDvFG1rL_hzzWH-29" edge="1"><mxGeometry x="-0.5" relative="1" as="geometry"><mxPoint as="offset"/></mxGeometry></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-36" value="&lt;div&gt;&lt;b&gt;OFF&lt;/b&gt;&lt;/div&gt;&lt;div&gt;enter / light_off();&lt;/div&gt;" style="rounded=1;whiteSpace=wrap;html=1;verticalAlign=top;align=left;spacingLeft=4;" parent="YyxwJ0gDvFG1rL_hzzWH-19" vertex="1"><mxGeometry x="110" y="115" width="200" height="60" as="geometry"/></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-37" value="" style="edgeStyle=orthogonalEdgeStyle;curved=1;rounded=0;comic=0;orthogonalLoop=1;jettySize=auto;html=0;fontColor=default;" parent="YyxwJ0gDvFG1rL_hzzWH-19" source="YyxwJ0gDvFG1rL_hzzWH-38" target="YyxwJ0gDvFG1rL_hzzWH-36" edge="1"><mxGeometry relative="1" as="geometry"/></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-38" value="$initial" style="ellipse;whiteSpace=wrap;html=1;fillColor=#000000;fontColor=none;noLabel=1;" parent="YyxwJ0gDvFG1rL_hzzWH-19" vertex="1"><mxGeometry x="195" y="55" width="30" height="30" as="geometry"/></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-39" value="DIM" style="edgeStyle=orthogonalEdgeStyle;curved=1;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=0.75;exitY=0;exitDx=0;exitDy=0;entryX=0.75;entryY=1;entryDx=0;entryDy=0;fontColor=default;" parent="YyxwJ0gDvFG1rL_hzzWH-19" source="YyxwJ0gDvFG1rL_hzzWH-29" target="YyxwJ0gDvFG1rL_hzzWH-36" edge="1"><mxGeometry x="-0.7" relative="1" as="geometry"><mxPoint as="offset"/></mxGeometry></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-41" value="INC" style="edgeStyle=orthogonalEdgeStyle;curved=1;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;fontColor=default;" parent="YyxwJ0gDvFG1rL_hzzWH-19" source="YyxwJ0gDvFG1rL_hzzWH-34" target="YyxwJ0gDvFG1rL_hzzWH-40" edge="1"><mxGeometry relative="1" as="geometry"/></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-40" value="&lt;div&gt;BOOM&lt;/div&gt;&lt;div&gt;enter / light_boom();&lt;/div&gt;" style="rounded=1;whiteSpace=wrap;html=1;verticalAlign=top;align=left;spacingLeft=4;fontStyle=0" parent="YyxwJ0gDvFG1rL_hzzWH-19" vertex="1"><mxGeometry x="110" y="675" width="200" height="60" as="geometry"/></mxCell><mxCell id="YyxwJ0gDvFG1rL_hzzWH-87" value="$choice" style="rhombus;fontColor=none;fillColor=#000000;noLabel=1;" parent="YyxwJ0gDvFG1rL_hzzWH-19" vertex="1"><mxGeometry x="360" y="55" width="30" height="30" as="geometry"/></mxCell></root></mxGraphModel>
            """;

        var actual = DrawIoDecoder.DecodeSvgToOriginalDiagrams(new StringReader(svgFileStart)).Single().xml;
        Assert.Equal(expected, actual);
    }

    public static string GetDiagramsContentsRaw_First(string xml)
    {
        return DrawIoDecoder.GetDiagramsContentsRaw(xml).First().xml;
    }
}
