using System.Text.RegularExpressions;

#nullable enable

namespace StateSmith.Input.DrawIo;

public class MxCellSanitizer
{
    public static void SanitizeLabel(MxCell mxCell)
    {
        if (mxCell.label == null)
            return;

        mxCell.label = mxCell.label.Replace((char)160, ' '); // convert non-breaking space char to regular space

        if (mxCell.HasMatchingStyle("html", "1"))
        {
            var label = mxCell.label;
            label = SanitizeLabelHtml(label);
            mxCell.label = label;
        }
    }

    public static string SanitizeLabelHtml(string label)
    {
        label = Regex.Replace(label, @"</div>", "\n");
        label = Regex.Replace(label, @"</p>", "\n");
        label = Regex.Replace(label, @"<br>", "\n");
        label = Regex.Replace(label, @"<br/>", "\n");
        label = Regex.Replace(label, @"&nbsp;", " ");
        label = Regex.Replace(label, @"<[^>]*>", "");
        label = System.Web.HttpUtility.HtmlDecode(label);
        return label;
    }
}
