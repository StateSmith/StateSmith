using Xunit;
using StateSmithTest.Output.BalancedCoder1;
using StateSmith.Output.Gil.C99;
using StateSmith.Output.UserConfig;
using StateSmith.Output;
using System.IO;
using StateSmith.Runner;

namespace StateSmithTest.Output.Gil;

public class GilToC99Tests
{
    [Fact]
    public void Test()
    {
        string programText = AlgoBalanced1Tests.BuildExampleGilFile(skipIndentation:true, out var sm).ToString();

        RenderConfigVars renderConfig = new();
        RenderConfigCVars renderConfigC = new();

        OutputInfo outputInfo = new()
        {
            outputDirectory = Path.GetTempPath(),
            //outputDirectory = TestHelper.GetThisDir() // use this one when troubleshooting
        };

        FilePathPrinter pathPrinter = new(outputInfo.outputDirectory);
        GilToC99 gilToC = new(outputInfo, new GilToC99Customizer(new StateMachineProvider(sm), renderConfigC), new CodeFileWriter(new StringBufferConsolePrinter(), pathPrinter), renderConfig, renderConfigC);

        gilToC.TranspileAndOutputCode(programText);
    }
}
