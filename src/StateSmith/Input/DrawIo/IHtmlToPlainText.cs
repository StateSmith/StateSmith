#nullable enable

namespace StateSmith.Input.DrawIo;

public interface IHtmlToPlainText
{
    string Convert(string html);
}
