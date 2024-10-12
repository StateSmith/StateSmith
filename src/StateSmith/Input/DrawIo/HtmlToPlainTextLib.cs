#nullable enable

using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text;

namespace StateSmith.Input.DrawIo;

/// <summary>
/// Modified from https://stackoverflow.com/a/30088920/7331858
/// See https://github.com/StateSmith/StateSmith/issues/414
/// </summary>
public class HtmlToPlainTextLib : IHtmlToPlainText
{
    public string Convert(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var builder = new StringBuilder();
        var state = ToPlainTextState.StartLine;

        Plain(builder, ref state, new[] { doc.DocumentNode });
        return builder.ToString();
    }

    static void Plain(StringBuilder builder, ref ToPlainTextState state, IEnumerable<HtmlNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is HtmlTextNode htmlTextNode)
            {
                var text = htmlTextNode.Text;
                Process(builder, ref state, HtmlEntity.DeEntitize(text).ToCharArray());
            }
            else
            {
                var tag = node.Name.ToLower();

                if (tag == "br")
                {
                    builder.Append('\n'); // prefer '\n' over Environment.NewLine
                    state = ToPlainTextState.StartLine;
                }
                else if (BlockTags.Contains(tag))
                {
                    if (state != ToPlainTextState.StartLine)
                    {
                        builder.Append('\n');
                        state = ToPlainTextState.StartLine;
                    }
                    Plain(builder, ref state, node.ChildNodes);
                    if (state != ToPlainTextState.StartLine && node.NextSibling != null)
                    {
                        builder.Append('\n');
                        state = ToPlainTextState.StartLine;
                    }
                }
                else
                {
                    // inline tags. Just process the children.
                    Plain(builder, ref state, node.ChildNodes);
                }
            }
        }
    }

    public static void Process(StringBuilder builder, ref ToPlainTextState state, params char[] chars)
    {
        foreach (var ch in chars)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (IsHardSpace(ch))
                {
                    if (state == ToPlainTextState.WhiteSpace)
                        builder.Append(' ');
                    builder.Append(' ');
                    state = ToPlainTextState.NotWhiteSpace;
                }
                else
                {
                    if (state == ToPlainTextState.NotWhiteSpace)
                        state = ToPlainTextState.WhiteSpace;
                }
            }
            else
            {
                if (state == ToPlainTextState.WhiteSpace)
                    builder.Append(' ');
                builder.Append(ch);
                state = ToPlainTextState.NotWhiteSpace;
            }
        }
    }
    static bool IsHardSpace(char ch)
    {
        return ch == 0xA0 || ch == 0x2007 || ch == 0x202F;
    }

    private static readonly HashSet<string> BlockTags = new()
    {
        // from https://developer.mozilla.org/en-US/docs/Glossary/Block-level_content
        "div", "p", "hr", "pre", "table", "h1", "h2", "h3", "h4", "h5", "h6", "blockquote", "ol", "ul", "li", "dl", "dt", "dd",
        "figure", "figcaption", "main", "nav", "section", "article", "aside", "header", "footer", "address", "fieldset", "legend", "form", "details", "summary",
    };

    public enum ToPlainTextState
    {
        StartLine,
        NotWhiteSpace,
        WhiteSpace,
    }
}
