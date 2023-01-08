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
        SetStyle(mxCell);
        SanitizeLabel(mxCell);

        mxCells.Add(mxCell.id, mxCell);
    }

    private void SetStyle(MxCell mxCell)
    {
        mxCell.style = MaybeGetAttribute("style");
        if (mxCell.style == null)
            return;

        var matches = Regex.Matches(mxCell.style, @"(?x)
            (\w+) #
            \s*
            (?:
                =
                \s*
                ([^;]*?)
                \s*
            )?
            (?: ;|$ )
        ");

        foreach (Match item in matches)
        {
            mxCell.styleMap.Add(item.Groups[1].Value, item.Groups[2].Value);
        }
    }

    private bool IsImage()
    {
        return MaybeGetAttribute("style")?.Contains("shape=image") ?? false;
    }

    private static void SanitizeLabel(MxCell mxCell)
    {
        if (mxCell.label == null)
            return;

        mxCell.label = mxCell.label.Replace((char)160, ' '); // convert non-breaking space char to regular space

        if (mxCell.HasMatchingStyle("html", "1"))
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