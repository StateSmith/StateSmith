using FluentAssertions;
using StateSmith.Runner;
using Xunit;

namespace StateSmithTest.Runner;

public class SmTransformerTest
{
    [Fact]
    public void TestSwap()
    {
        var transformer = new SmTransformer();
        transformer.transformationPipeline.Add(new TransformationStep("a", (sm) => { }));
        transformer.transformationPipeline.Add(new TransformationStep("b", (sm) => { }));

        transformer.GetMatchIndex("a").Should().Be(0);
        transformer.GetMatchIndex("b").Should().Be(1);

        transformer.Swap("a", "b");
        
        transformer.GetMatchIndex("a").Should().Be(1);
        transformer.GetMatchIndex("b").Should().Be(0);
    }
}
