using Xunit;
using FluentAssertions;
using StateSmith.Compiling;
using StateSmith.compiler.Visitors;
using System.Linq;
using StateSmith.Runner;

namespace StateSmithTest;

public class SmToStatesVisitorTest
{
    [Fact]
    public void First()
    {
        CompilerRunner compilerRunner = ExamplesTestHelpers.SetupTiny2Sm();

        SmToNamedVerticesVisitor visitor = new();
        compilerRunner.FindSingleStateMachine();
        compilerRunner.sm.Accept(visitor);

        visitor.namedVertices.Count.Should().Be(7);

        int i = 0;
        visitor.namedVertices[i++].Name.Should().Be("Tiny2"); // same as ROOT
        visitor.namedVertices[i++].Name.Should().Be("S1");
        visitor.namedVertices[i++].Name.Should().Be("S1_1");
        visitor.namedVertices[i++].Name.Should().Be("S1_1_1");
        visitor.namedVertices[i++].Name.Should().Be("S1_1_2");
        visitor.namedVertices[i++].Name.Should().Be("S1_2");
        visitor.namedVertices[i++].Name.Should().Be("S2");
    }
}
