using FluentAssertions;
using StateSmith.Runner;
using StateSmithTest;
using Xunit;

namespace StateSmithCliTest;

public class TemplateRenderTest
{
    [Fact]
    public void LineFilterSingle()
    {
        var r = new TemplateRenderer(TargetLanguageId.C, "0.8.1-alpha", "../../MySm.drawio");

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
        var r = new TemplateRenderer(TargetLanguageId.CppC, "0.8.1-alpha", "../../MySm.drawio");

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
        var r = new TemplateRenderer(TargetLanguageId.CppC, "0.8.1-alpha", "../../MySm.drawio");

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
        var r = new TemplateRenderer(TargetLanguageId.CppC, "0.8.1-alpha", "../../MySm.drawio");

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
        var r = new TemplateRenderer(TargetLanguageId.C, "0.8.1-alpha", "../../MySm.drawio");

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
        var r = new TemplateRenderer(TargetLanguageId.CppC, "0.8.1-alpha", "../../MySm.drawio");

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
        var r = new TemplateRenderer(TargetLanguageId.CppC, "0.8.1-alpha", "../../MySm.drawio");

        const string Template = """
            SmRunner runner = new(diagramPath: "{{diagramPath}}", new MyRenderConfig(), transpilerId: {{transpilerId}});
            runner.Run();
            """;
        r.SetTemplate(Template);

        r.Render().ShouldBeShowDiff("""
            SmRunner runner = new(diagramPath: "../../MySm.drawio", new MyRenderConfig(), transpilerId: {{transpilerId}});
            runner.Run();
            """);
    }
}
