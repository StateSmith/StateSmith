using Xunit;
using StateSmith.Output.Gil.C99;
using StateSmith.Output.UserConfig;
using StateSmith.Output;
using System.Text;
using FluentAssertions;
using System.Linq;

namespace StateSmithTest.Output.C99;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/112
/// </summary>
public class IncludeGuardProviderTests_112
{
    [Fact]
    public void Test_pragma_once()
    {
        IncludeGuardProvider includeGuardProvider = MakeIncludeGuardProvider("", "GameSm");
        AssertPragmaOnce(includeGuardProvider);
    }

    [Fact]
    public void Test_pragma_once_whitespace()
    {
        IncludeGuardProvider includeGuardProvider = MakeIncludeGuardProvider(" ", "GameSm");
        AssertPragmaOnce(includeGuardProvider);
    }

    private static void AssertPragmaOnce(IncludeGuardProvider includeGuardProvider)
    {
        StringBuilder hFileSb = new();

        includeGuardProvider.OutputIncludeGuardTop(hFileSb);
        hFileSb.ToString().ShouldBeShowDiff("""
            #pragma once  // You can also specify normal include guard. See https://github.com/StateSmith/StateSmith/issues/112

            """);

        hFileSb.Clear();
        includeGuardProvider.OutputIncludeGuardBottom(hFileSb);
        hFileSb.ToString().ShouldBeShowDiff("");
    }

    [Fact]
    public void Test_FILENAME()
    {
        IncludeGuardProvider includeGuardProvider = MakeIncludeGuardProvider("{FILENAME}_H", "RocketSm");
        StringBuilder hFileSb = new();

        includeGuardProvider.OutputIncludeGuardTop(hFileSb);
        hFileSb.ToString().ShouldBeShowDiff("""
            #ifndef ROCKETSM_H
            #define ROCKETSM_H

            """);
        hFileSb.Clear();

        includeGuardProvider.OutputIncludeGuardBottom(hFileSb);
        hFileSb.ToString().ShouldBeShowDiff("#endif // ROCKETSM_H\n");
    }

    [Fact]
    public void Test_fileName()
    {
        IncludeGuardProvider includeGuardProvider = MakeIncludeGuardProvider("{fileName}_H", "GuiSm");
        StringBuilder hFileSb = new();

        includeGuardProvider.OutputIncludeGuardTop(hFileSb);
        hFileSb.ToString().ShouldBeShowDiff("""
            #ifndef GuiSm_H
            #define GuiSm_H

            """);
        hFileSb.Clear();

        includeGuardProvider.OutputIncludeGuardBottom(hFileSb);
        hFileSb.ToString().ShouldBeShowDiff("#endif // GuiSm_H\n");
    }

    [Fact]
    public void Test_custom()
    {
        IncludeGuardProvider includeGuardProvider = MakeIncludeGuardProvider("MY_CUSTOM_INCLUDE_GUARD", "GuiSm");
        StringBuilder hFileSb = new();

        includeGuardProvider.OutputIncludeGuardTop(hFileSb);
        hFileSb.ToString().ShouldBeShowDiff("""
            #ifndef MY_CUSTOM_INCLUDE_GUARD
            #define MY_CUSTOM_INCLUDE_GUARD

            """);
        hFileSb.Clear();

        includeGuardProvider.OutputIncludeGuardBottom(hFileSb);
        hFileSb.ToString().ShouldBeShowDiff("#endif // MY_CUSTOM_INCLUDE_GUARD\n");
    }

    [Fact]
    public void IntegrationTest_pragma_once()
    {
        var plantUmlText = """
            @startuml ExampleSm
            [*] --> s1

            /'! $CONFIG: toml
            SmRunnerSettings.transpilerId = "C99"
            '/
            @enduml
            """;

        var fakeFs = new CapturingCodeFileWriter();
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter: fakeFs);
        var code = fakeFs.GetCapturesForFileName("ExampleSm.h").Single().code;

        code.Should().Contain("""#pragma once""");
        code.Should().Contain("""https://github.com/StateSmith/StateSmith/issues/112""");
    }

    [Fact]
    public void IntegrationTest_FILENAME()
    {
        var plantUmlText = """
            @startuml ExampleSm
            [*] --> s1

            /'! $CONFIG: toml
            RenderConfig.C.IncludeGuardLabel = "{FILENAME}_H"
            SmRunnerSettings.transpilerId = "C99"
            '/
            @enduml
            """;

        var fakeFs = new CapturingCodeFileWriter();
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter: fakeFs);
        var code = fakeFs.GetCapturesForFileName("ExampleSm.h").Single().code;

        code.Should().Contain("""#ifndef EXAMPLESM_H""");
        code.Should().Contain("""#define EXAMPLESM_H""");
        code.Should().Contain("""#endif // EXAMPLESM_H""");
    }

    [Fact]
    public void IntegrationTest_Custom()
    {
        var plantUmlText = """
            @startuml ExampleSm
            [*] --> s1

            /'! $CONFIG: toml
            RenderConfig.C.IncludeGuardLabel = "MyCustomIncludeGuardLabel_h"
            SmRunnerSettings.transpilerId = "C99"
            '/
            @enduml
            """;

        var fakeFs = new CapturingCodeFileWriter();
        TestHelper.CaptureRunSmRunnerForPlantUmlString(plantUmlText, codeFileWriter: fakeFs);
        var code = fakeFs.GetCapturesForFileName("ExampleSm.h").Single().code;

        code.Should().Contain("""#ifndef MyCustomIncludeGuardLabel_h""");
        code.Should().Contain("""#define MyCustomIncludeGuardLabel_h""");
        code.Should().Contain("""#endif // MyCustomIncludeGuardLabel_h""");
    }

    private static IncludeGuardProvider MakeIncludeGuardProvider(string IncludeGuardLabel, string baseFileName)
    {
        OutputInfo outputInfo = new();
        RenderConfigCVars renderConfigC = new();

        renderConfigC.IncludeGuardLabel = IncludeGuardLabel;
        outputInfo.baseFileName = baseFileName;
        IncludeGuardProvider includeGuardProvider = new(renderConfigC, outputInfo);
        return includeGuardProvider;
    }
}
