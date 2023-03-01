using Xunit;
using StateSmithTest.Output.BalancedCoder1;
using StateSmith.Output.Gil.C99;
using StateSmith.Output.UserConfig;
using StateSmith.Output.C99BalancedCoder1;
using StateSmith.Output;
using System.IO;

namespace StateSmithTest.Output.Gil;

public class GilToC99Tests
{
    [Fact]
    public void Test()
    {
        string programText = AlgoBalanced1Tests.BuildExampleGilFile(skipIndentation:true, out var sm).ToString();

        RenderConfigCVars renderConfigC = new();

        OutputInfo outputInfo = new()
        {
            outputDirectory = Path.GetTempPath(),
            //outputDirectory = TestHelper.GetThisDir() // use this one when troubleshooting
        };
        CNameMangler cNameMangler = new(sm);

        File.WriteAllText($"{outputInfo.outputDirectory}{cNameMangler.SmName}.gil.cs", programText);

        GilToC99 gilToC = new(renderConfigC, outputInfo, cNameMangler);

        gilToC.TranspileAndOutputCode(programText);
    }
}
