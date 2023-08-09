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
