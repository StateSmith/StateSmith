using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

#nullable enable

namespace StateSmith.Input.DrawIo;

public class MxCellParser
{
    protected readonly XmlTextReader reader;

    public Dictionary<string, MxCell> mxCells = new();

    public MxCellParser(string filepath)
    {
        reader = new XmlTextReader(filepath);
    }

    public MxCell GetCellById(string id)
    {
        return mxCells[id];
    }

    public MxCellParser(TextReader textReader)
    {
        reader = new XmlTextReader(textReader);
    }

    public void Parse()
    {
        while (reader.Read())
        {
            HandleRead();
        }
    }

    private void HandleRead()
    {
        if (reader.IsStartElement("mxCell"))
        {
            HandleMxCell();
        }
    }

    private void HandleMxCell()
    {
        var mxCell = new MxCell(GetAttributeOrThrow("id"));

        if (HasAttribute("vertex"))
        {
            mxCell.type = MxCell.Type.Vertex;
            if (IsImage())
            {
                mxCell.type = MxCell.Type.VertexImage;
            }
        }
        else if (HasAttribute("edge"))
        {
            mxCell.type = MxCell.Type.Edge;
        }
        else
        {
            //throw new DrawIoException("Unknown mxcell type. Expected edge or vertex.");
        }

        mxCell.parent = MaybeGetAttribute("parent");
        mxCell.label = MaybeGetAttribute("value");
        mxCell.source = MaybeGetAttribute("source");
        mxCell.target = MaybeGetAttribute("target");
        RemoveAnyHtmlTags(mxCell);

        mxCells.Add(mxCell.id, mxCell);
    }

    private bool IsImage()
    {
        return MaybeGetAttribute("style")?.Contains("shape=image") ?? false;
    }

    private void RemoveAnyHtmlTags(MxCell mxCell)
    {
        var style = MaybeGetAttribute("style");
        if (style == null)
            return;

        var maybeQuote = "\"?";
        if (mxCell.label != null && Regex.IsMatch(style, $@"html\s*=\s*{maybeQuote}1{maybeQuote}"))
        {
            var label = mxCell.label;
            label = Regex.Replace(label, @"</div>", "\n");
            label = Regex.Replace(label, @"&nbsp;", " ");
            label = Regex.Replace(label, @"<[^>]*>", "");
            label = System.Web.HttpUtility.HtmlDecode(label);
            mxCell.label = label;
        }
    }

    private string GetAttributeOrThrow(string attributeName)
    {
        string? attr = MaybeGetAttribute(attributeName);
        if (attr == null)
        {
            throw new DrawIoException($"failed getting attribute {attributeName} from xml element.");
        }
        return attr;
    }

    private bool HasAttribute(string attributeName)
    {
        return reader.GetAttribute(attributeName) != null;
    }

    private string? MaybeGetAttribute(string attributeName)
    {
        return reader.GetAttribute(attributeName);
    }
}