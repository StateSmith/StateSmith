#nullable enable

using Xunit;
using StateSmithTest.Output.BalancedCoder1;
using StateSmith.Output.UserConfig;
using StateSmith.Output;
using StateSmith.Output.Gil.JavaScript;
using FluentAssertions;

namespace StateSmithTest.Output.Gil;

public class GilToJavaScriptTests
{
    [Fact]
    public void Test()
    {
        string gilCode = AlgoBalanced1Tests.BuildExampleGilFile(skipIndentation: false, out var sm).ToString();

        RenderConfigJavaScriptVars renderConfig = new()
        {
            UseExportOnClass = true,
            ExtendsSuperClass = "MyBaseClass"
        };

        OutputInfo outputInfo = new()
        {
            outputDirectory = TestHelper.GetThisDir()
        };

        //File.WriteAllText($"{outputInfo.outputDirectory}temp.gil.cs", gilCode);

        CapturingCodeFileWriter capturingWriter = new();
        GilToJavaScript transpiler = new(outputInfo, new(sm), new(), renderConfig, capturingWriter);

        transpiler.TranspileAndOutputCode(gilCode);

        capturingWriter.code.Should().Contain("export class TestsMySm1 extends MyBaseClass\n{");
    }
}
