using System.Linq;
using System.Text.RegularExpressions;

namespace StateSmith.Cli.Create;

/// <summary>
/// This class allows for filtering of templates based on tags like `C` or `CppC`, etc.
/// See unit tests for examples.
/// </summary>
public class TemplateFilterEngine
{
    /// <summary>
    /// Must have capture groups named 'tags' and 'content'
    /// </summary>
    static readonly Regex multilineFilter = new(@"(?xm)
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

    /// <summary>
    /// Must have capture groups named 'tags' and 'content'
    /// </summary>
    static readonly Regex inlineFilter = new(@"(?x)
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

    /// <summary>
    /// Must have capture groups named 'tags' and 'content'
    /// </summary>
    static readonly Regex lineRegex = new(@"(?x)
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

    public string ProcessAllFilters(string template, string filterTag)
    {
        template = ProcessMultiLineFilters(template, filterTag);
        template = ProcessInlineFilters(template, filterTag);
        template = ProcessLineFilters(template, filterTag);

        return template;
    }

    public string ProcessMultiLineFilters(string template, string filterTag)
    {
        template = ProcessFilterRegexForTag(regex: multilineFilter, template: template, filterTag: filterTag);
        return template;
    }

    public string ProcessInlineFilters(string template, string filterTag)
    {
        template = ProcessFilterRegexForTag(regex: inlineFilter, template: template, filterTag: filterTag);
        return template;
    }

    public string ProcessLineFilters(string template, string filterTag)
    {
        template = ProcessFilterRegexForTag(regex: lineRegex, template: template, filterTag: filterTag);
        return template;
    }

    protected static string ProcessFilterRegexForTag(Regex regex, string template, string filterTag)
    {
        template = regex.Replace(template, (match) =>
        {
            var tags = match.Groups["tags"].Value.Split(',');
            var content = match.Groups["content"].Value;

            if (tags.Contains(filterTag))
            {
                return content;
            }

            return "";
        });
        return template;
    }
}
