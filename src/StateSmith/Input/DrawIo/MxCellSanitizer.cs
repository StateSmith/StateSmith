using System.Text.RegularExpressions;

#nullable enable

namespace StateSmith.Input.DrawIo;

public class MxCellSanitizer
{
    public const string StyleHtml = "html";

    public static void SanitizeLabel(MxCell mxCell)
    {
        if (mxCell.label == null)
            return;

        mxCell.label = ProcessSpecialChars(mxCell.label);

        if (mxCell.HasMatchingStyle(StyleHtml, "1"))
        {
            var label = mxCell.label;
            label = SanitizeLabelHtml(label);
            mxCell.label = label;
        }
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

    public static string SanitizeLabelHtml(string label)
    {
        label = Regex.Replace(label, @"</div>", "\n");
        label = Regex.Replace(label, @"</p>", "\n");
        label = Regex.Replace(label, @"(?x) < /? br [^>]* >", "\n"); // (?x) verbose regex mode. br tags can have many differnt forms: https://www.tutorialspoint.com/What-is-the-correct-way-of-using-br-br-or-br-in-HTML
        label = Regex.Replace(label, @"&nbsp;", " ");
        label = Regex.Replace(label, @"<[^>]*>", "");
        label = System.Web.HttpUtility.HtmlDecode(label);
        return label;
    }
}
