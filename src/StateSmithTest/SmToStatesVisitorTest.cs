using Xunit;
using FluentAssertions;
using StateSmith.Compiling;
using StateSmith.compiler.Visitors;
using System.Linq;

namespace StateSmithTest
{
    public class SmToStatesVisitorTest
    {
        [Fact]
        public void First()
        {
            Compiler compiler = ExamplesTestHelpers.SetupTiny2Sm();

            SmToNamedVerticesVisitor v = new();
            compiler.rootVertices.Single().Accept(v);

            v.namedVertices.Count.Should().Be(7);

            int i = 0;
            v.namedVertices[i++].Name.Should().Be("Tiny2"); // same as ROOT
            v.namedVertices[i++].Name.Should().Be("S1");
            v.namedVertices[i++].Name.Should().Be("S1_1");
            v.namedVertices[i++].Name.Should().Be("S1_1_1");
            v.namedVertices[i++].Name.Should().Be("S1_1_2");
            v.namedVertices[i++].Name.Should().Be("S1_2");
            v.namedVertices[i++].Name.Should().Be("S2");
        }
    }
}
