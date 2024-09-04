#nullable enable

using Xunit;
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
        string programText = GilFileTestHelper.BuildExampleGilFile(skipIndentation:true, out var sm).ToString();

        RenderConfigBaseVars renderConfig = new();
        RenderConfigCVars renderConfigC = new();

        OutputInfo outputInfo = new()
        {
            outputDirectory = Path.GetTempPath(),
            //outputDirectory = TestHelper.GetThisDir() // use this one when troubleshooting
        };

        FilePathPrinter pathPrinter = new(outputInfo.outputDirectory);
        GilToC99 gilToC = new(outputInfo, new GilToC99Customizer(renderConfigC, outputInfo), new CodeFileWriter(new StringBufferConsolePrinter(), pathPrinter), renderConfig, renderConfigC, new());

        gilToC.TranspileAndOutputCode(programText);
    }
}
