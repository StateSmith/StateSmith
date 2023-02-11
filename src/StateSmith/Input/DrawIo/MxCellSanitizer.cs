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
        input = input.Replace((char)160, ' '); // convert non-breaking space char to regular space
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
