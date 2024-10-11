#nullable enable

using System.Text.RegularExpressions;

namespace StateSmith.Input.DrawIo;

public class HtmlToPlainTextRegex : IHtmlToPlainText
{
    public string Convert(string html)
    {
        html = Regex.Replace(html, @"</div>", "\n");
        html = Regex.Replace(html, @"</p>", "\n");
        html = Regex.Replace(html, @"(?x) < /? br [^>]* >", "\n"); // (?x) verbose regex mode. br tags can have many differnt forms: https://www.tutorialspoint.com/What-is-the-correct-way-of-using-br-br-or-br-in-HTML
        html = Regex.Replace(html, @"&nbsp;", " ");
        html = Regex.Replace(html, @"<[^>]*>", "");
        html = System.Web.HttpUtility.HtmlDecode(html);
        return html;
    }
}
