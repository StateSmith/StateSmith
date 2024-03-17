using FluentAssertions;
using StateSmith.Runner;
using StateSmithCli.Create;
using StateSmithTest;
using Xunit;

namespace StateSmithCliTest;

public class TemplateRenderTest
{
    [Fact]
    public void LineFilterSingle()
    {
        var r = new CsxTemplateRenderer(TargetLanguageId.C, "0.8.1-alpha", "../../MySm.drawio");

        const string Template = """
            Header stuff
            C stuff     //!!<line-filter:C>
            C++ stuff   //!!<line-filter:CppC>
            C# stuff    //!!<line-filter:CSharp>
            js stuff    //!!<line-filter:JavaScript>
            Footer stuff
            """;
        r.SetTemplate(Template);

        r.Render().ShouldBeShowDiff("""
            Header stuff
            C stuff
            Footer stuff
            """);
    }

    [Fact]
    public void LineFilterMulti()
    {
        var r = new CsxTemplateRenderer(TargetLanguageId.CppC, "0.8.1-alpha", "../../MySm.drawio");

        const string Template = """
            Header 
            C stuff     //!!<line-filter:C>
            C/C++ stuff //!!<line-filter:C,CppC>
            C++ stuff   //!!<line-filter:CppC>
            C# stuff    //!!<line-filter:CSharp>
            js stuff    //!!<line-filter:JavaScript>
            Footer stuff
            """;
        r.SetTemplate(Template);

        r.Render().ShouldBeShowDiff("""
            Header 
            C/C++ stuff
            C++ stuff
            Footer stuff
            """);
    }

    [Fact]
    public void MultiLineFilter()
    {
        var r = new CsxTemplateRenderer(TargetLanguageId.CppC, "0.8.1-alpha", "../../MySm.drawio");

        const string Template = """
            Header 
            //!!<filter:CppC>
            // NOTE!!! Idiomatic C++ code generation is coming. This will improve.
            // See https://github.com/StateSmith/StateSmith/issues/126
            string IRenderConfigC.CFileExtension => "".cpp""; // the generated StateSmith C code is also valid C++ code
            string IRenderConfigC.HFileExtension => "".h"";   // could also be .hh, .hpp or whatever you like
            //!!</filter>
            Footer stuff
            """;
        r.SetTemplate(Template);

        r.Render().ShouldBeShowDiff("""
            Header 
            // NOTE!!! Idiomatic C++ code generation is coming. This will improve.
            // See https://github.com/StateSmith/StateSmith/issues/126
            string IRenderConfigC.CFileExtension => "".cpp""; // the generated StateSmith C code is also valid C++ code
            string IRenderConfigC.HFileExtension => "".h"";   // could also be .hh, .hpp or whatever you like
            Footer stuff
            """);
    }

    [Fact]
    public void MultiLineFilter_MultiTag()
    {
        var r = new CsxTemplateRenderer(TargetLanguageId.CppC, "0.8.1-alpha", "../../MySm.drawio");

        const string Template = """
            Header 
            //!!<filter:JavaScript,CppC>
            // NOTE!!! Idiomatic C++ code generation is coming. This will improve.
            // See https://github.com/StateSmith/StateSmith/issues/126
            string IRenderConfigC.CFileExtension => "".cpp""; // the generated StateSmith C code is also valid C++ code
            string IRenderConfigC.HFileExtension => "".h"";   // could also be .hh, .hpp or whatever you like
            //!!</filter>
            Footer stuff
            """;
        r.SetTemplate(Template);

        r.Render().ShouldBeShowDiff("""
            Header 
            // NOTE!!! Idiomatic C++ code generation is coming. This will improve.
            // See https://github.com/StateSmith/StateSmith/issues/126
            string IRenderConfigC.CFileExtension => "".cpp""; // the generated StateSmith C code is also valid C++ code
            string IRenderConfigC.HFileExtension => "".h"";   // could also be .hh, .hpp or whatever you like
            Footer stuff
            """);
    }

    [Fact]
    public void MultiLineFilter_Negative()
    {
        var r = new CsxTemplateRenderer(TargetLanguageId.C, "0.8.1-alpha", "../../MySm.drawio");

        const string Template = """
            Header 
            //!!<filter:CppC>
            // NOTE!!! Idiomatic C++ code generation is coming. This will improve.
            // See https://github.com/StateSmith/StateSmith/issues/126
            string IRenderConfigC.CFileExtension => "".cpp""; // the generated StateSmith C code is also valid C++ code
            string IRenderConfigC.HFileExtension => "".h"";   // could also be .hh, .hpp or whatever you like
            //!!</filter>
            Footer stuff
            """;
        r.SetTemplate(Template);

        r.Render().ShouldBeShowDiff("""
            Header 
            Footer stuff
            """);
    }


    [Fact]
    public void ReplaceStateSmithVersion()
    {
        var r = new CsxTemplateRenderer(TargetLanguageId.CppC, "0.8.1-alpha", "../../MySm.drawio");

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
        var r = new CsxTemplateRenderer(TargetLanguageId.CppC, "0.8.1-alpha", "../../MySm.drawio");

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

    [Fact]
    public void IntegrationTest()
    {
        var csxTemplate = TemplateLoader.LoadCsx("drawio-simple-1");

        var r = new CsxTemplateRenderer(TargetLanguageId.C, "0.9.9-alpha", "../../MySm.drawio", template: csxTemplate);

        var result = r.Render();

        result.ShouldBeShowDiff("""
            #!/usr/bin/env dotnet-script
            // If you have any questions about this file, check out https://github.com/StateSmith/tutorial-2
            #r "nuget: StateSmith, 0.9.9-alpha"

            using StateSmith.Common;
            using StateSmith.Input.Expansions;
            using StateSmith.Output.UserConfig;
            using StateSmith.Runner;

            // See https://github.com/StateSmith/tutorial-2/blob/main/lesson-1/
            SmRunner runner = new(diagramPath: "../../MySm.drawio", new MyRenderConfig(), transpilerId: TranspilerId.C99);
            runner.Run();

            // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-2
            public class MyRenderConfig : IRenderConfig
            {

                // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-3
                public class MyExpansions : UserExpansionScriptBase
                {
                }
            }
            
            """);
    }
}
