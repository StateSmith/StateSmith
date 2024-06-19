#nullable enable

using FluentAssertions;
using StateSmith.Output;
using StateSmith.Output.Gil.C99;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph;
using System.Linq;
using Xunit;

namespace StateSmithTest.Output.Gil;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/185
/// </summary>
public class GilToC99CustomizerTests
{
    private readonly RenderConfigCVars renderConfigC = new();
    private readonly GilToC99Customizer customizer;

    public GilToC99CustomizerTests()
    {
        var outputInfo = new OutputInfo()
        {
            outputDirectory = "null",
            baseFileName = "MySuperSm",
        };
        customizer = new GilToC99Customizer(renderConfigC, outputInfo);
    }

    [Fact]
    public void GccExample()
    {
        renderConfigC.CEnumDeclarer = "typedef enum __attribute__((packed)) {enumName}";
        customizer.Setup();
        customizer.EnumDeclarationBuilder("SomeEnumType").Should().Be("typedef enum __attribute__((packed)) SomeEnumType");
    }

    [Fact]
    public void SubstitutionVarWhiteSpace()
    {
        renderConfigC.CEnumDeclarer = "some stuff { enumName } __END_STUFF";
        customizer.Setup();
        customizer.EnumDeclarationBuilder("MyEnumType").Should().Be("some stuff MyEnumType __END_STUFF");
    }

    [Fact]
    public void GccIntegrationTest()
    {
        var (smRunner, capturingWriter) = TestHelper.CaptureSmRun("ExGil1.drawio", transpilerId: TranspilerId.C99);

        // Note that draw.io file already contains the equivalent of the below
        //class ExampleRenderConfig : IRenderConfigC
        //{
        //    string IRenderConfigC.CEnumDeclarer => "typedef enum __attribute__((packed)) {enumName}";
        //    string IRenderConfigC.CFileExtension => ".cpp";
        //    string IRenderConfigC.HFileExtension => ".hpp";
        //}

        var cppFile = capturingWriter.captures.GetValues(smRunner.Settings.outputDirectory + "ExGil1.cpp").Single();
        var hppFile = capturingWriter.captures.GetValues(smRunner.Settings.outputDirectory + "ExGil1.hpp").Single();

        hppFile.code.Should().Contain("typedef enum __attribute__((packed)) ExGil1_StateId");
        hppFile.code.Should().Contain("typedef enum __attribute__((packed)) ExGil1_EventId");
    }
}
