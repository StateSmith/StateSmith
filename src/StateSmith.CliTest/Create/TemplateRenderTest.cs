using FluentAssertions;
using StateSmith.Cli.Create;
using StateSmithTest;
using Xunit;

namespace StateSmithCliTest.Create;

public class TemplateRenderTest
{
    private CsxTemplateRenderer MakeRenderer(TargetLanguageId targetLanguageId, string stateSmithVersion, string diagramPath)
    {
        return new CsxTemplateRenderer(targetLanguageId: targetLanguageId, stateSmithVersion: stateSmithVersion, diagramPath: diagramPath, smName: "MySm");
    }

    [Fact]
    public void ReplaceStateSmithVersion()
    {
        var r = MakeRenderer(TargetLanguageId.CppC, "0.8.1-alpha", "../../MySm.drawio");

        const string Template = """
            #!/usr/bin/env dotnet-script
            #r "nuget: StateSmith, {{stateSmithVersion}}"
            
            using StateSmith.Common;
            """;
        r.SetTemplate(Template);

        r.Render().ShouldBeShowDiff("""
            #!/usr/bin/env dotnet-script
            #r "nuget: StateSmith, 0.8.1-alpha"

            using StateSmith.Common;
            """);
    }

    [Fact]
    public void ReplaceDiagramPath()
    {
        var r = MakeRenderer(TargetLanguageId.CppC, "0.8.1-alpha", "../../MySm.drawio");

        const string Template = """
            SmRunner runner = new(diagramPath: "{{diagramPath}}", new MyRenderConfig(), transpilerId: {{transpilerId}});
            runner.Run();
            """;
        r.SetTemplate(Template);

        r.Render().ShouldBeShowDiff("""
            SmRunner runner = new(diagramPath: "../../MySm.drawio", new MyRenderConfig(), transpilerId: C99);
            runner.Run();
            """);
    }


    // todolow - consider reorganizing integration tests. Should they test Generator instead?


    [Fact]
    public void IntegrationTestTomlC()
    {
        // TODO do for more languages
        var tomlConfig = Generator.GetTomlConfig(TargetLanguageId.C, TemplateLoader.TomlConfigType.Most);

        tomlConfig.ShouldBeShowDiff(""""
            ############# Render Config Settings ##############

            [RenderConfig]
            FileTop = """
                // Whatever you put in this `FileTop` section will end up 
                // being printed at the top of every generated code file.
                """
            # AutoExpandedVars = ""
            # VariableDeclarations = ""
            # DefaultVarExpTemplate = ""
            # DefaultFuncExpTemplate = ""
            # DefaultAnyExpTemplate = ""
            # TriggerMap = ""

            [RenderConfig.C]
            # CFileExtension = ".inc"
            # HFileExtension = ".h"
            # HFileTop = ""
            # HFileIncludes = "#include <stdlib.h>"
            # CFileIncludes = """#include "some_header.h" """
            # CFileTop = ""
            # CEnumDeclarer = "typedef enum __attribute__((packed)) {enumName}"

            ############# SmRunner.Settings ###############

            [SmRunnerSettings]
            # transpilerId = "C99"
            # outputDirectory = "./gen"
            # outputCodeGenTimestamp = true
            # outputStateSmithVersionInfo = false
            # propagateExceptions = true
            # dumpErrorsToFile = true

            [SmRunnerSettings.smDesignDescriber]
            # enabled = true
            # outputDirectory = ".."
            # outputAncestorHandlers = true

            [SmRunnerSettings.smDesignDescriber.outputSections]
            # beforeTransformations = false
            # afterTransformations  = true

            [SmRunnerSettings.algoBalanced1]
            # outputEventIdToStringFunction = false
            # outputStateIdToStringFunction = false

            [SmRunnerSettings.simulation]
            # enableGeneration = false
            # outputDirectory = ".."
            # outputFileNamePostfix = ".sim.html"

            # There are more SmRunnerSettings. See C# classes on github project.
            # See https://github.com/StateSmith/StateSmith/blob/main/src/StateSmith/Runner/RunnerSettings.cs
            """", outputCleanActual: true);
    }

    [Fact]
    public void IntegrationTestC()
    {
        var csxTemplate = TemplateLoader.LoadDefaultCsx();
        var r = new CsxTemplateRenderer(TargetLanguageId.C, "0.9.9-alpha", "../../RocketSm.drawio", smName: "RocketSm", template: csxTemplate);
        var result = r.Render();

        result.ShouldBeShowDiff(""""
            #!/usr/bin/env dotnet-script
            // If you have any questions about this file, check out https://github.com/StateSmith/tutorial-2
            #r "nuget: StateSmith, 0.9.9-alpha"

            using StateSmith.Common;
            using StateSmith.Input.Expansions;
            using StateSmith.Output.UserConfig;
            using StateSmith.Runner;

            // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-1
            SmRunner runner = new(diagramPath: "../../RocketSm.drawio", new MyRenderConfig(), transpilerId: TranspilerId.C99);
            runner.Run();

            // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-2/ (basics)
            // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-5/ (language specific options)
            public class MyRenderConfig : IRenderConfigC
            {
                string IRenderConfig.FileTop => """
                    // Whatever you put in the IRenderConfig.FileTop section ends up at the top of the generated file(s).
                    """;

                string IRenderConfig.AutoExpandedVars => """
                    int auto_expanded_user_example_var;
                    """;

                string IRenderConfig.VariableDeclarations => """
                    int non_expanded_user_example_var;
                    """;

                string IRenderConfigC.HFileTop => """
                    // user IRenderConfigC.HFileTop: whatever you want to put in here.
                    """;

                string IRenderConfigC.CFileTop => """
                    // user IRenderConfigC.CFileTop: whatever you want to put in here.
                    //#include <stdio.h> // or whatever you need
                    """;

                string IRenderConfigC.HFileIncludes => """
                    // user IRenderConfigC.HFileIncludes: whatever you want to put in here.
                    """;

                string IRenderConfigC.CFileIncludes => """
                    // user IRenderConfigC.CFileIncludes: whatever you want to put in here.
                    """;

                // Optional: customize how enumerations are declared so that gcc will use the smallest possible int type instead of a full int.
                // string IRenderConfigC.CEnumDeclarer => "typedef enum __attribute__ ((packed)) {enumName}";

                // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-3
                public class MyExpansions : UserExpansionScriptBase
                {
                    // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-4 for timing expansions
                }
            }
            
            """", outputCleanActual: true);
    }

    [Fact]
    public void IntegrationTestCpp()
    {
        var csxTemplate = TemplateLoader.LoadDefaultCsx();
        var r = new CsxTemplateRenderer(TargetLanguageId.CppC, "0.9.9-alpha", "../../RocketSm.drawio", smName: "RocketSm", template: csxTemplate);
        var result = r.Render();

        result.ShouldBeShowDiff(""""
            #!/usr/bin/env dotnet-script
            // If you have any questions about this file, check out https://github.com/StateSmith/tutorial-2
            #r "nuget: StateSmith, 0.9.9-alpha"

            using StateSmith.Common;
            using StateSmith.Input.Expansions;
            using StateSmith.Output.UserConfig;
            using StateSmith.Runner;

            // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-1
            SmRunner runner = new(diagramPath: "../../RocketSm.drawio", new MyRenderConfig(), transpilerId: TranspilerId.C99);
            runner.Run();

            // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-2/ (basics)
            // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-5/ (language specific options)
            public class MyRenderConfig : IRenderConfigC
            {
                // NOTE!!! Idiomatic C++ code generation is coming. This will improve.
                // See https://github.com/StateSmith/StateSmith/issues/126
                string IRenderConfigC.CFileExtension => ".cpp"; // the generated StateSmith C code is also valid C++ code
                string IRenderConfigC.HFileExtension => ".h";   // could also be .hh, .hpp or whatever you like

                string IRenderConfig.FileTop => """
                    // Whatever you put in the IRenderConfig.FileTop section ends up at the top of the generated file(s).
                    """;

                string IRenderConfig.AutoExpandedVars => """
                    int auto_expanded_user_example_var;
                    """;

                string IRenderConfig.VariableDeclarations => """
                    int non_expanded_user_example_var;
                    """;

                string IRenderConfigC.HFileTop => """
                    // user IRenderConfigC.HFileTop: whatever you want to put in here.
                    """;

                string IRenderConfigC.CFileTop => """
                    // user IRenderConfigC.CFileTop: whatever you want to put in here.
                    //#include <stdio.h> // or whatever you need
                    """;

                string IRenderConfigC.HFileIncludes => """
                    // user IRenderConfigC.HFileIncludes: whatever you want to put in here.
                    """;

                string IRenderConfigC.CFileIncludes => """
                    // user IRenderConfigC.CFileIncludes: whatever you want to put in here.
                    """;

                // Optional: customize how enumerations are declared so that gcc will use the smallest possible int type instead of a full int.
                // string IRenderConfigC.CEnumDeclarer => "typedef enum __attribute__ ((packed)) {enumName}";

                // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-3
                public class MyExpansions : UserExpansionScriptBase
                {
                    // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-4 for timing expansions
                }
            }
            
            """");
    }


    [Fact]
    public void IntegrationTestCSharp()
    {
        var csxTemplate = TemplateLoader.LoadDefaultCsx();
        var r = new CsxTemplateRenderer(TargetLanguageId.CSharp, "0.9.9-alpha", "../../RocketSm.drawio", smName: "RocketSm", template: csxTemplate);
        var result = r.Render();

        result.ShouldBeShowDiff(""""
            #!/usr/bin/env dotnet-script
            // If you have any questions about this file, check out https://github.com/StateSmith/tutorial-2
            #r "nuget: StateSmith, 0.9.9-alpha"

            using StateSmith.Common;
            using StateSmith.Input.Expansions;
            using StateSmith.Output.UserConfig;
            using StateSmith.Runner;

            // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-1
            SmRunner runner = new(diagramPath: "../../RocketSm.drawio", new MyRenderConfig(), transpilerId: TranspilerId.CSharp);
            runner.Run();

            // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-2/ (basics)
            // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-5/ (language specific options)
            public class MyRenderConfig : IRenderConfigCSharp
            {
                string IRenderConfig.FileTop => """
                    // Whatever you put in the IRenderConfig.FileTop section ends up at the top of the generated file(s).
                    """;

                // Usings for your state machine so that it can access other code.
                string IRenderConfigCSharp.Usings => """
                    using System;  // or whatever you need
                    """;

                // Namespace for your generated state machine class.
                // If this ends with a ";" it will use a top level namespace instead of using braces.
                string IRenderConfigCSharp.NameSpace => "RocketSm;";

                // Makes the generated state machine extend a _user_ provided class `RocketSmBase`.
                // Not required, but can be handy if you don't want to use expansions.
                // string IRenderConfigCSharp.BaseList => "RocketSmBase";

                // Instead of extending a base class, you could mark the generated state machine class
                // as a partial class. You could do both if you want.
                bool IRenderConfigCSharp.UsePartialClass => true;

                // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-3
                public class MyExpansions : UserExpansionScriptBase
                {
                    // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-4 for timing expansions
                }
            }
            
            """");
    }

    [Fact]
    public void IntegrationTestJavaScript()
    {
        var csxTemplate = TemplateLoader.LoadDefaultCsx();
        var r = new CsxTemplateRenderer(TargetLanguageId.JavaScript, "0.9.9-alpha", "../../RocketSm.drawio", smName: "RocketSm", template: csxTemplate);
        var result = r.Render();

        result.ShouldBeShowDiff(""""
            #!/usr/bin/env dotnet-script
            // If you have any questions about this file, check out https://github.com/StateSmith/tutorial-2
            #r "nuget: StateSmith, 0.9.9-alpha"

            using StateSmith.Common;
            using StateSmith.Input.Expansions;
            using StateSmith.Output.UserConfig;
            using StateSmith.Runner;

            // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-1
            SmRunner runner = new(diagramPath: "../../RocketSm.drawio", new MyRenderConfig(), transpilerId: TranspilerId.JavaScript);
            runner.Run();

            // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-2/ (basics)
            // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-5/ (language specific options)
            public class MyRenderConfig : IRenderConfigJavaScript
            {
                string IRenderConfig.FileTop => """
                    "use strict";
                    // Whatever you put in the IRenderConfig.FileTop section ends up at the top of the generated file(s).
                    """;

                string IRenderConfig.AutoExpandedVars => """
                    auto_expanded_user_example_var : 0,
                    """;

                string IRenderConfig.VariableDeclarations => """
                    non_expanded_user_example_var : 0,
                    """;

                // Base class not needed. Sometimes handy though so showing it here.
                // string IRenderConfigJavaScript.ExtendsSuperClass => "RocketSmBase";

                // Enable if you want generated state machine class like `export class RocketSm...`
                // bool IRenderConfigJavaScript.UseExportOnClass => true;

                // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-3
                public class MyExpansions : UserExpansionScriptBase
                {
                    // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-4 for timing expansions
                }
            }
            
            """");
    }

    [Fact]
    public void RenderConfigTestC()
    {
        var csxTemplate = TemplateLoader.LoadDefaultCsx();
        var r = new CsxTemplateRenderer(TargetLanguageId.C, "0.9.9-alpha", "../../MySm.drawio", smName: "MySm", template: csxTemplate);
        var result = r.Render();

        result.Should().Contain("""
            public class MyRenderConfig : IRenderConfigC
            {
            """);
    }

    [Fact]
    public void RenderConfigTestCpp()
    {
        var csxTemplate = TemplateLoader.LoadDefaultCsx();
        var r = new CsxTemplateRenderer(TargetLanguageId.CppC, "0.9.9-alpha", "../../MySm.drawio", smName: "MySm", template: csxTemplate);
        var result = r.Render();

        result.Should().Contain("""
            public class MyRenderConfig : IRenderConfigC
            {
            """);
    }

    [Fact]
    public void RenderConfigTestCSharp()
    {
        var csxTemplate = TemplateLoader.LoadDefaultCsx();
        var r = new CsxTemplateRenderer(TargetLanguageId.CSharp, "0.9.9-alpha", "../../MySm.drawio", smName: "MySm", template: csxTemplate);
        var result = r.Render();

        result.Should().Contain("""
            public class MyRenderConfig : IRenderConfigCSharp
            {
            """);
    }

    [Fact]
    public void RenderConfigTestJavaScript()
    {
        var csxTemplate = TemplateLoader.LoadDefaultCsx();
        var r = new CsxTemplateRenderer(TargetLanguageId.JavaScript, "0.9.9-alpha", "../../MySm.drawio", smName: "MySm", template: csxTemplate);
        var result = r.Render();

        result.Should().Contain("""
            public class MyRenderConfig : IRenderConfigJavaScript
            {
            """);
    }

    [Fact]
    public void TestCppFilter()
    {
        var csxTemplate = TemplateLoader.LoadDefaultCsx();
        var r = new CsxTemplateRenderer(TargetLanguageId.CppC, "0.9.9-alpha", "../../MySm.drawio", smName: "MySm", template: csxTemplate);
        var result = r.Render();

        result.Should().Contain("""string IRenderConfigC.CFileExtension => ".cpp";""");
        result.Should().Contain("""string IRenderConfigC.HFileExtension => ".h";""");
    }
}
