using System.Linq;
using System.Text.RegularExpressions;

namespace StateSmithCli.Create;

public class TemplateRenderer
{
    protected TargetLanguageId targetLanguageId;
    protected string stateSmithVersion;
    protected string diagramPath;
    protected string template;

    public TemplateRenderer(TargetLanguageId targetLanguageId, string stateSmithVersion, string diagramPath, string template = "")
    {
        this.targetLanguageId = targetLanguageId;
        this.stateSmithVersion = stateSmithVersion;
        this.diagramPath = diagramPath;
        this.template = template;
    }

    public void SetTemplate(string template)
    {
        this.template = template;
    }

    public string Render()
    {
        var str = template.Replace("{{stateSmithVersion}}", stateSmithVersion).Replace("{{diagramPath}}", diagramPath);

        var transpilerId = targetLanguageId switch
        {
            TargetLanguageId.C => "C99",
            TargetLanguageId.CppC => "C99",
            TargetLanguageId.CSharp => "CSharp",
            TargetLanguageId.JavaScript => "JavaScript",
            _ => throw new System.NotImplementedException(),
        };

        str = str.Replace("{{transpilerId}}", transpilerId);

        var langKeyFilterKey = targetLanguageId switch
        {
            TargetLanguageId.C => "C",
            TargetLanguageId.CppC => "CppC",
            TargetLanguageId.CSharp => "CSharp",
            TargetLanguageId.JavaScript => "JavaScript",
            _ => throw new System.NotImplementedException(),
        };

        str = ReplaceMultiLineFilters(str, langKeyFilterKey);

        str = ReplaceLineFilters(str, langKeyFilterKey);

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


        str = multilineFilter.Replace(str, (match) =>
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

        str = lineRegex.Replace(str, (match) =>
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
}
