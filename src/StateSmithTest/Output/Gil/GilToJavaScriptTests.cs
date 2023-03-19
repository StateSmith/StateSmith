using Xunit;
using StateSmithTest.Output.BalancedCoder1;
using StateSmith.Output.Gil.C99;
using StateSmith.Output.UserConfig;
using StateSmith.Output;
using System.IO;
using StateSmith.Output.Gil.JavaScript;

namespace StateSmithTest.Output.Gil;

public class GilToJavaScriptTests
{
    [Fact]
    public void Test()
    {
        string gilCode = AlgoBalanced1Tests.BuildExampleGilFile(skipIndentation: false, out var sm).ToString();

        RenderConfigJavaScriptVars renderConfig = new();
        renderConfig.UseExportOnClass = true;

        OutputInfo outputInfo = new()
        {
            outputDirectory = Path.GetTempPath(),
            //outputDirectory = TestHelper.GetThisDir() // use this one when troubleshooting
        };

        //File.WriteAllText($"{outputInfo.outputDirectory}temp.gil.cs", gilCode);

        GilToJavaScript transpiler = new(outputInfo, new(sm), new(), renderConfig, new CodeFileWriter());

        transpiler.TranspileAndOutputCode(gilCode);
    }
}
