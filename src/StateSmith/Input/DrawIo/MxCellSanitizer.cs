#nullable enable

using StateSmith.Runner;
using System;

namespace StateSmith.Input.DrawIo;

public class MxCellSanitizer
{
    public const string StyleHtml = "html";

    public static void SanitizeLabel(MxCell mxCell, IConsolePrinter consolePrinter)
    {
        if (mxCell.label == null)
            return;

        if (mxCell.HasMatchingStyle(StyleHtml, "1"))
        {
            var label = mxCell.label;
            label = HtmlToPlainText(label, consolePrinter);
            mxCell.label = label;
        }

        // process special characters after converting html to plain text
        // because some of the special characters are detected.
        mxCell.label = ProcessSpecialChars(mxCell.label);
    }

    public static string ProcessSpecialChars(string input)
    {
        // https://en.wikipedia.org/wiki/Non-breaking_space#Encodings
        // https://www.compart.com/en/unicode/category/Zs
        input = input.Replace((char)0xA0, ' '); // NO-BREAK SPACE, &nbsp;
        input = input.Replace((char)0x1680, ' '); // OGHAM SPACE MARK
        input = input.Replace((char)0x2000, ' '); // EN QUAD
        input = input.Replace((char)0x2001, ' '); // EM QUAD
        input = input.Replace((char)0x2002, ' '); // EN SPACE
        input = input.Replace((char)0x2003, ' '); // EM SPACE
        input = input.Replace((char)0x2004, ' '); // THREE-PER-EM SPACE
        input = input.Replace((char)0x2005, ' '); // FOUR-PER-EM SPACE
        input = input.Replace((char)0x2006, ' '); // SIX-PER-EM SPACE
        input = input.Replace((char)0x2007, ' '); // FIGURE SPACE
        input = input.Replace((char)0x2008, ' '); // PUNCTUATION SPACE
        input = input.Replace((char)0x2009, ' '); // THIN SPACE
        input = input.Replace((char)0x200A, ' '); // HAIR SPACE
        input = input.Replace((char)0x202F, ' '); // NARROW NO-BREAK SPACE
        input = input.Replace((char)0x205F, ' '); // MEDIUM MATHEMATICAL SPACE
        input = input.Replace((char)0x2060, ' '); // WORD JOINER
        input = input.Replace((char)0x3000, ' '); // IDEOGRAPHIC SPACE

        return input;
    }

    public static string HtmlToPlainText(string html, IConsolePrinter consolePrinter)
    {
        try
        {
            return new HtmlToPlainTextLib().Convert(html);
        }
        catch (Exception e)
        {
            // I don't actually expect this to happen, but I'd rather have a fallback while we send it out to users for testing.
            // TODOLOW - remove this fallback once we're confident that HtmlToPlainTextLib is working.
            consolePrinter.WriteErrorLine("!!! Please report this issue. HtmlToPlainTextLib failed. Falling back to HtmlToPlainTextRegex.");
            consolePrinter.WriteErrorLine(e.ToString());
            return new HtmlToPlainTextRegex().Convert(html);
        }
    }
}
