using StateSmith.Output.UserConfig;
using System.Linq;
using System.Text.RegularExpressions;

namespace StateSmith.Cli.Create;

public class TemplateRenderer
{
    protected TargetLanguageId targetLanguageId;
    protected string smName;
    protected string stateSmithVersion;
    protected string diagramPath;
    protected string template;

    public TemplateRenderer(TargetLanguageId targetLanguageId, string stateSmithVersion, string diagramPath, string smName, string template = "")
    {
        this.targetLanguageId = targetLanguageId;
        this.stateSmithVersion = stateSmithVersion;
        this.diagramPath = diagramPath;
        this.template = template;
        this.smName = smName;
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
        var str = template.Replace("{{stateSmithVersion}}", stateSmithVersion);
        str = str.Replace("{{diagramPath}}", diagramPath);
        str = str.Replace("{{smName}}", smName);

        var transpilerId = targetLanguageId switch
        {
            TargetLanguageId.C => "C99",
            TargetLanguageId.CppC => "C99",
            TargetLanguageId.CSharp => "CSharp",
            TargetLanguageId.JavaScript => "JavaScript",
            _ => throw new System.NotImplementedException(),
        };
        str = str.Replace("{{transpilerId}}", transpilerId);

        var renderConfigBase = targetLanguageId switch
        {
            TargetLanguageId.C => nameof(IRenderConfigC),
            TargetLanguageId.CppC => nameof(IRenderConfigC),
            TargetLanguageId.CSharp => nameof(IRenderConfigCSharp),
            TargetLanguageId.JavaScript => nameof(IRenderConfigJavaScript),
            _ => throw new System.NotImplementedException(),
        };
        str = str.Replace("{{renderConfigBase}}", renderConfigBase);

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
        Regex multilineFilter = new(@"(?xm)
            (?: [ \t]* (?:\r\n|\r|\n) *)*   # leading whitespace
            //!!<filter:
            (?<tags>
                [^>]+
            )
            >
            (?<content>
                (?:.|\r\n|\r|\n)
            *?)
            (?:\r\n|\r|\n|^)?  [ \t]*  # leading whitespace
            //!!<\/filter>
            [ \t]*
            (?: (?:\r\n|\r|\n) [ \t]* $)*   # blank lines
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
                (?:.|\r\n|\r|\n)
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
