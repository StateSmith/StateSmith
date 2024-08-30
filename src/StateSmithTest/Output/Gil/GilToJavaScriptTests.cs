#nullable enable

using Xunit;
using StateSmith.Output.UserConfig;
using StateSmith.Output;
using StateSmith.Output.Gil.JavaScript;
using FluentAssertions;
using StateSmith.Output.Gil;

namespace StateSmithTest.Output.Gil;

public class GilToJavaScriptTests
{
    [Fact]
    public void Test()
    {
        string gilCode = AlgoTests.BuildExampleGilFile(skipIndentation: false, out var sm).ToString();

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
        GilToJavaScript transpiler = new(outputInfo, new(), renderConfig, capturingWriter, new());

        transpiler.TranspileAndOutputCode(gilCode);

        capturingWriter.LastCode.Should().Contain("export class TestsMySm1 extends MyBaseClass\n{");
    }
}
