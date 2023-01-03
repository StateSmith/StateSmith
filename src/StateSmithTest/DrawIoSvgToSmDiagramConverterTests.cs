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
        string filePath = ExamplesTestHelpers.ExamplesInputDirectoryPath + "Tutorial1-complete/src/Tutorial1Sm.drawio.svg";
        DrawIoToSmDiagramConverter converter = new();
        converter.ProcessSvg(File.OpenText(filePath));

        var smDiagramRoot = converter.Roots.Single();
        smDiagramRoot.label.Should().Be("$STATEMACHINE: Tutorial1Sm");
        smDiagramRoot.children.Count.Should().Be(4);
        int i = 0;
        smDiagramRoot.children[i].label.Should().Be("ON_GROUP");
        smDiagramRoot.children[i].parent.Should().Be(smDiagramRoot);
        smDiagramRoot.children[i].children.Count.Should().Be(3);

        // could do more tests here for all nodes and edges

        CompilerRunner runner = new();
        runner.CompileNodesToVertices(converter.Roots, converter.Edges);
        runner.FindSingleStateMachine();
        runner.sm.Name.Should().Be("Tutorial1Sm");
    }

    [Fact]
    public void CompileDrawIoSvgFileNodesToVertices()
    {
        string filePath = ExamplesTestHelpers.ExamplesInputDirectoryPath + "Tutorial1-complete/src/Tutorial1Sm.drawio.svg";

        CompilerRunner runner = new();
        runner.CompileDrawIoFileNodesToVertices(filePath);
        runner.FindSingleStateMachine();

        runner.sm.Name.Should().Be("Tutorial1Sm");
    }
}

