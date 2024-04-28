using System.Linq;
using System.Text.RegularExpressions;

namespace StateSmith.Cli.Create;

public class CsxTemplateRenderer
{
    protected TargetLanguageId targetLanguageId;
    protected string stateSmithVersion;
    protected string diagramPath;
    protected string template;

    public CsxTemplateRenderer(TargetLanguageId targetLanguageId, string stateSmithVersion, string diagramPath, string template = "")
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

    private static string ConvertPathsToForwardSlashes(string path)
    {
        return path.Replace('\\', '/');
    }

    public string Render()
    {
        diagramPath = ConvertPathsToForwardSlashes(diagramPath);  // we do this so that we don't need to escape backslashes in the generated string
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
        str = ReplaceInlineFilters(str, langKeyFilterKey);
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
            (?<content>
                (?:.|\n)
            *?)
            (?:\r\n|\r|\n|^)?  [ \t]*  # leading whitespace
            //!!<\/filter>
            [ \t]*
        ");

        str = ReplaceContentForTags(str, langKey, multilineFilter);
        return str;
    }

    private static string ReplaceInlineFilters(string str, string langKey)
    {
        Regex inlineFilter = new(@"(?x)
            (?:\r\n|\r|\n|^)?  [ \t]*  # leading whitespace
            /[*]!!<filter:
            (?<tags>
                [^>]+
            )
            >[*]/
            (?<content>
                (?:.|\n)
            *?)
            (?:\r\n|\r|\n|^)?  [ \t]*  # leading whitespace
            /[*]!!<\/filter>[*]/
            [ \t]*
        ");

        str = ReplaceContentForTags(str, langKey, inlineFilter);
        return str;
    }

    private static string ReplaceLineFilters(string str, string langKey)
    {
        Regex lineRegex = new(@"(?x)
            (?<content>
                (?:\r\n|\r|\n|^)    # line break or start of string
                .+?                 # line content before filter
            )
            [ \t]*
            //!!<line-filter:
            (?<tags>
                [^>]+
            )
            >
        .*");

        str = ReplaceContentForTags(str, langKey, lineRegex);
        return str;
    }


    private static string ReplaceContentForTags(string str, string langKey, Regex regex)
    {
        str = regex.Replace(str, (match) =>
        {
            var tags = match.Groups["tags"].Value.Split(',');
            var content = match.Groups["content"].Value;

            if (tags.Contains(langKey))
            {
                return content;
            }

            return "";
        });
        return str;
    }
}
