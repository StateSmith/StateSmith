using Xunit;
using StateSmithTest.Output.BalancedCoder1;
using StateSmith.Output.Gil.C99;
using StateSmith.Output.UserConfig;
using StateSmith.Output.C99BalancedCoder1;
using StateSmith.Output;

namespace StateSmithTest.Output.Gil;

public class GilToC99Tests
{
    [Fact]
    public void Runxyz()
    {
        string programText = AlgoBalanced1Tests.BuildExampleGilFile(skipIndentation:true, out var sm).ToString();

        RenderConfigC renderConfigC = new();

        OutputInfo outputInfo = new()
        {
            outputDirectory = TestHelper.GetThisDir()
        };

        GilToC99 gilToC = new(renderConfigC, outputInfo, new CNameMangler(sm));
        gilToC.TranspileAndOutputCode(programText);
    }
}
