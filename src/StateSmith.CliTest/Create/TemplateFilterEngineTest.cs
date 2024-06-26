using StateSmith.Cli.Create;
using StateSmithTest;
using Xunit;

namespace StateSmithCliTest.Create;

public class TemplateFilterEngineTest
{
    TemplateFilterEngine filterEngine = new();

    private void TestExpectation(TargetLanguageId targetLanguageId, string template, string expected)
    {
        var result = filterEngine.ProcessAllFilters(template, targetLanguageId.ToString());
        result.ShouldBeShowDiff(expected);
    }

    [Fact]
    public void LineFilterSingle()
    {
        TargetLanguageId targetLanguageId = TargetLanguageId.C;

        const string template = """
            Header stuff
            C stuff     //!!<line-filter:C>
            C++ stuff   //!!<line-filter:CppC>
            C# stuff    //!!<line-filter:CSharp>
            js stuff    //!!<line-filter:JavaScript>
            Footer stuff
            """;

        const string expected = """
            Header stuff
            C stuff
            Footer stuff
            """;

        TestExpectation(targetLanguageId, template: template, expected: expected);
    }

    [Fact]
    public void LineFilterMulti()
    {
        TargetLanguageId targetLanguageId = TargetLanguageId.CppC;

        const string Template = """
            Header 
            C stuff     //!!<line-filter:C>
            C/C++ stuff //!!<line-filter:C,CppC>
            C++ stuff   //!!<line-filter:CppC>
            C# stuff    //!!<line-filter:CSharp>
            js stuff    //!!<line-filter:JavaScript>
            Footer stuff
            """;

        const string expected = """
            Header 
            C/C++ stuff
            C++ stuff
            Footer stuff
            """;
        
        TestExpectation(targetLanguageId, template: Template, expected: expected);
    }

    [Fact]
    public void MultiLineFilter()
    {
        TargetLanguageId targetLanguageId = TargetLanguageId.CppC;

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

        const string expected = """
            Header
            // NOTE!!! Idiomatic C++ code generation is coming. This will improve.
            // See https://github.com/StateSmith/StateSmith/issues/126
            string IRenderConfigC.CFileExtension => "".cpp""; // the generated StateSmith C code is also valid C++ code
            string IRenderConfigC.HFileExtension => "".h"";   // could also be .hh, .hpp or whatever you like
            Footer stuff
            """;

        TestExpectation(targetLanguageId, template: Template, expected: expected);
    }

    [Fact]
    public void MultiLineFilter_MultiTag()
    {
        TargetLanguageId targetLanguageId = TargetLanguageId.CppC;

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

        const string expected = """
            Header
            // NOTE!!! Idiomatic C++ code generation is coming. This will improve.
            // See https://github.com/StateSmith/StateSmith/issues/126
            string IRenderConfigC.CFileExtension => "".cpp""; // the generated StateSmith C code is also valid C++ code
            string IRenderConfigC.HFileExtension => "".h"";   // could also be .hh, .hpp or whatever you like
            Footer stuff
            """;

        TestExpectation(targetLanguageId, template: Template, expected: expected);
    }

    [Fact]
    public void MultiLineFilter_Negative()
    {
        TargetLanguageId targetLanguageId = TargetLanguageId.C;

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

        const string expected = """
            Header
            Footer stuff
            """;

        TestExpectation(targetLanguageId, template: Template, expected: expected);
    }

    [Fact]
    public void InlineFilter()
    {
        TargetLanguageId targetLanguageId;
        string expected;

        const string Template = """
            public class MyRenderConfig : IRenderConfig /*!!<filter:CppC>*/, IRenderConfigC/*!!</filter>*/
            """;

        // test matching filter
        targetLanguageId = TargetLanguageId.CppC;
        expected = """
            public class MyRenderConfig : IRenderConfig, IRenderConfigC
            """;
        TestExpectation(targetLanguageId, template: Template, expected: expected);

        // test non-matching filter
        targetLanguageId = TargetLanguageId.C;
        expected = """
            public class MyRenderConfig : IRenderConfig
            """;
        TestExpectation(targetLanguageId, template: Template, expected: expected);
    }
}
