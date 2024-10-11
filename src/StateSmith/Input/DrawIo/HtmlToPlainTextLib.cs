#nullable enable

using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text;

namespace StateSmith.Input.DrawIo;

/// <summary>
/// Modified from https://stackoverflow.com/a/30088920/7331858
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
                    builder.Append('\n'); // prefer \n over Environment.NewLine
                    state = ToPlainTextState.StartLine;
                }
                else if (NonVisibleTags.Contains(tag))
                {
                }
                else if (InlineTags.Contains(tag))
                {
                    Plain(builder, ref state, node.ChildNodes);
                }
                else
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
            }
        }
    }

    //common part
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

    private static readonly HashSet<string> InlineTags = new HashSet<string>
    {
      //from https://developer.mozilla.org/en-US/docs/Web/HTML/Inline_elemente
      "b", "big", "i", "small", "tt", "abbr", "acronym",
      "cite", "code", "dfn", "em", "kbd", "strong", "samp",
      "var", "a", "bdo", "br", "img", "map", "object", "q",
      "script", "span", "sub", "sup", "button", "input", "label",
      "select", "textarea", "font"
    };

    private static readonly HashSet<string> NonVisibleTags = new HashSet<string>
    {
          "script", "style"
    };

    public enum ToPlainTextState
    {
        StartLine = 0,
        NotWhiteSpace,
        WhiteSpace,
    }
}
