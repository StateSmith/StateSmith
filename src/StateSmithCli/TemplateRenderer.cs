#nullable enable

using System.Linq;
using System.Text.RegularExpressions;

namespace StateSmith.Runner;

public class TemplateRenderer
{
    protected TargetLanguageId targetLanguageId;
    protected string stateSmithVersion;
    protected string diagramPath;
    protected string template = CsxWithTiming;

    public TemplateRenderer(TargetLanguageId targetLanguageId, string stateSmithVersion, string diagramPath)
    {
        this.targetLanguageId = targetLanguageId;
        this.stateSmithVersion = stateSmithVersion;
        this.diagramPath = diagramPath;
    }

    public void SetTemplate(string template)
    {
        this.template = template;
    }

    public string Render()
    {
        var str = template.Replace("{{stateSmithVersion}}", stateSmithVersion).Replace("{{diagramPath}}", diagramPath);

        var langKey = targetLanguageId switch
        {
            TargetLanguageId.C => "C",
            TargetLanguageId.CppC => "CppC",
            TargetLanguageId.CSharp => "CSharp",
            TargetLanguageId.JavaScript => "JavaScript",
            _ => throw new System.NotImplementedException(),
        };

        str = ReplaceMultiLineFilters(str, langKey);

        str = ReplaceLineFilters(str, langKey);

        return str;
    }

    private static string ReplaceMultiLineFilters(string str, string langKey)
    {
        Regex multilineFilter = new(@"(?x)
            (?:\r\n|\r|\n|^)?  [ \t]*  # leading whitespace
            //!!<filter:
            (?<tags>
                [^>]+
            )
            >
            (?<inner>
                (?:.|\n)
            *?)
            (?:\r\n|\r|\n|^)?  [ \t]*  # leading whitespace
            //!!<\/filter>
            [ \t]*
        ");


        str = multilineFilter.Replace(str, (Match match) =>
        {
            var tags = match.Groups["tags"].Value.Split(',');
            var inner = match.Groups["inner"].Value;

            if (tags.Contains(langKey))
            {
                return inner;
            }

            return "";
        });
        return str;
    }

    private static string ReplaceLineFilters(string str, string langKey)
    {
        Regex lineRegex = new(@"(?x)
            (?<line>
                (?:\r\n|\r|\n|^)    # line break or start of string
                .+                  # line content before filter
            )
            //!!<line-filter:
            (?<tags>
                [^>]+
            )
            >
        .*");

        str = lineRegex.Replace(str, (Match match) =>
        {
            var tags = match.Groups["tags"].Value.Split(',');
            var line = match.Groups["line"].Value.TrimEnd();

            if (tags.Contains(langKey))
            {
                return line;
            }

            return "";
        });
        return str;
    }

    public const string CsxWithTiming = @"
#!/usr/bin/env dotnet-script
#r ""nuget: StateSmith, {{stateSmithVersion}}""

using StateSmith.Common;
using StateSmith.Input.Expansions;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;

// See https://github.com/StateSmith/tutorial-2/blob/main/lesson-1/
SmRunner runner = new(diagramPath: ""{{diagramPath}}"", new MyRenderConfig(), transpilerId: {{transpilerId}});
runner.Run();

// See https://github.com/StateSmith/tutorial-2/tree/main/lesson-2
public class MyRenderConfig : IRenderConfig
{
    // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-4
    string IRenderConfig.VariableDeclarations => @""
        uint32_t t1_start_ms; // tracks when timer was started //!!<line-filter:C,CppC>
        long t1_start_ms; // tracks when timer was started //!!<line-filter:CSharp>
        t1_start_ms: 0, // tracks when timer was started //!!<line-filter:JavaScript>
    "";

    //!!<filter:CppC>
    // NOTE!!! Idiomatic C++ code generation is coming. This will improve.
    // See https://github.com/StateSmith/StateSmith/issues/126
    string IRenderConfigC.CFileExtension => "".cpp""; // the generated StateSmith C code is also valid C++ code
    string IRenderConfigC.HFileExtension => "".h"";   // could also be .hh, .hpp or whatever you like
    //!!</filter>

    // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-3
    public class MyExpansions : UserExpansionScriptBase
    {
        // see https://github.com/StateSmith/tutorial-2/tree/main/lesson-4
        string now_ms => $""you_need_to_specify_how_to_get_milliseconds()""; //NOTE! Update here for your language/framework. Could be a function call or reference a state machine variable...
        string t1_start_ms => AutoVarName();
        string t1_restart() => $""{t1_start_ms} = {now_ms}"";
        string t1_elapsed_ms => $""{now_ms} - {t1_start_ms}"";
        string t1_after(string timeStr) => $""{t1_elapsed_ms} >= {TimeStrToMs(timeStr)}"";
    }

    // this method takes a string like ""1.2s"" and converts it to `1200` for milliseconds.
    public static string TimeStrToMs(string timeStr)
    {
        return TimeStringParser.ElapsedTimeStringToMs(timeStr).ToString();
    }
}
";

}
