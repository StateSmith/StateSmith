using System.IO;
using Xunit;
using StateSmith.Input.DrawIo;
using System.Linq;
using FluentAssertions;
using StateSmith.Runner;

namespace StateSmithTest.DrawIo;

public class DrawIoSvgToSmDiagramConverterTests
{
    [Fact]
    public void Test()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/Design1Sm.drawio.svg";
        DrawIoToSmDiagramConverter converter = new();
        converter.ProcessSvg(File.OpenText(filePath));

        var smDiagramRoot = converter.Roots.Single();
        smDiagramRoot.label.Should().Be("$STATEMACHINE: Design1Sm_svg");
        smDiagramRoot.children.Count.Should().Be(4);
        int i = 0;
        smDiagramRoot.children[i].label.Should().Be("ON_GROUP");
        smDiagramRoot.children[i].parent.Should().Be(smDiagramRoot);
        smDiagramRoot.children[i].children.Count.Should().Be(3);

        // could do more tests here for all nodes and edges

        CompilerRunner runner = new();
        runner.CompileNodesToVertices(converter.Roots, converter.Edges);
        runner.FindSingleStateMachine();
        runner.sm.Name.Should().Be("Design1Sm_svg");
    }

    [Fact]
    public void CompileDrawIoSvgFileNodesToVertices()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/Design1Sm.drawio.svg";

        CompilerRunner runner = new();
        runner.CompileDrawIoFileNodesToVertices(filePath);
        runner.FindSingleStateMachine();

        runner.sm.Name.Should().Be("Design1Sm_svg");
    }

    [Fact]
    public void CompileDrawIoFileNodesToVertices_Compressed()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/Design1Sm_compressed.drawio";
        File.ReadAllText(filePath).Should().NotContain("<mxGraphModel", because: "this file needs to be compressed");
        // Note that vscode extension tends to write file uncompressed, but draw.io windows app tends to write it compressed.

        CompilerRunner runner = new();
        runner.CompileDrawIoFileNodesToVertices(filePath);
        runner.FindSingleStateMachine();

        runner.sm.Name.Should().Be("Design1Sm_compressed");
    }

    [Fact]
    public void CompileDrawIoFileNodesToVertices_UncompressedFile()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/Design1Sm_regular.drawio";
        File.ReadAllText(filePath).Should().Contain("<mxGraphModel", because: "this should not be compressed");
        // Note that vscode extension tends to write file uncompressed, but draw.io windows app tends to write it compressed.

        CompilerRunner runner = new();
        runner.CompileDrawIoFileNodesToVertices(filePath);
        runner.FindSingleStateMachine();

        runner.sm.Name.Should().Be("Design1Sm_regular");
    }

    [Fact]
    public void CompileDrawIoFileNodesToVertices_UncompressedDioFile()
    {
        string filePath = ExamplesTestHelpers.TestInputDirectoryPath + "drawio/Design1Sm_regular_dio.dio";
        File.ReadAllText(filePath).Should().Contain("<mxGraphModel", because: "this should not be compressed");
        // Note that vscode extension tends to write file uncompressed, but draw.io windows app tends to write it compressed.

        CompilerRunner runner = new();
        runner.CompileDrawIoFileNodesToVertices(filePath);
        runner.FindSingleStateMachine();

        runner.sm.Name.Should().Be("Design1Sm_regular_dio");
    }
}

