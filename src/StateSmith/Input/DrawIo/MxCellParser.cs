using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.CodeAnalysis;
using StateSmith.Output;

#nullable enable

namespace StateSmith.Input.DrawIo;

/// <summary>
/// Parses XML from mxGraphModel element.
/// </summary>
public class MxCellParser
{
    protected readonly XmlTextReader reader;
    private readonly string xmlContents;
    public Dictionary<string, MxCell> mxCells = new();

    MxCell? lastVertexCell;
    int cellCount = 0; // useful as first 2 mxCell nodes are "root" like invisible special nodes.

    public MxCell GetCellById(string id)
    {
        return mxCells[id];
    }

    public MxCellParser(string xmlContents)
    {
        reader = new XmlTextReader(new StringReader(xmlContents));
        this.xmlContents = xmlContents;
    }

    public void Close()
    {
        reader.Close();
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
        else if (reader.IsStartElement("mxGeometry"))
        {
            HandleMxGeometry();
        }
        else if (reader.IsStartElement("mxRectangle"))
        {
            HandleMxRectangle();
        }
    }

    private void ParseBounds(MxBounds bounds)
    {
        bounds.x = GetAttributeDoubleOrDefault("x", 0);
        bounds.y = GetAttributeDoubleOrDefault("y", 0);
        bounds.width = GetAttributeDoubleOrDefault("width", 0);
        bounds.height = GetAttributeDoubleOrDefault("height", 0);
    }

    private void HandleMxCell()
    {
        lastVertexCell = null;
        var mxCell = new MxCell(GetAttributeOrThrow("id"));
        mxCell.isSpecialInvisbleRootTypeNode = cellCount < 2;
        cellCount++;

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

        if (mxCell.type == MxCell.Type.Vertex)
        {
            mxCell.isCollapsed = HasAttributeValue("collapsed", "1");
            lastVertexCell = mxCell;
        }

        mxCell.parent = MaybeGetAttribute("parent");
        mxCell.label = MaybeGetAttribute("value");
        mxCell.source = MaybeGetAttribute("source");
        mxCell.target = MaybeGetAttribute("target");
        SetStyle(mxCell);
        MxCellSanitizer.SanitizeLabel(mxCell);

        mxCells.Add(mxCell.id, mxCell);
    }

    internal void SetStyle(MxCell mxCell)
    {
        mxCell.styleString = MaybeGetAttribute("style");
        if (mxCell.styleString == null)
            return;

        var matches = Regex.Matches(mxCell.styleString, @"(?x)
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

        foreach (Match item in matches.Cast<Match>())
        {
            mxCell.SetStyle(item.Groups[1].Value, item.Groups[2].Value);
        }
    }

    private bool IsImage()
    {
        return MaybeGetAttribute("style")?.Contains("shape=image") ?? false;
    }

    private void HandleMxRectangle()
    {
        if (lastVertexCell == null || !HasAttributeValue("as", "alternateBounds"))
        {
            return;
        }

        lastVertexCell.alternateBounds ??= new MxBounds();
        ParseBounds(lastVertexCell.alternateBounds);
    }

    private void HandleMxGeometry()
    {
        if (lastVertexCell == null || !HasAttributeValue("as", "geometry"))
        {
            return;
        }

        lastVertexCell.primaryBounds ??= new MxBounds();
        ParseBounds(lastVertexCell.primaryBounds);
    }

    private string GetAttributeOrThrow(string attributeName)
    {
        string? attr = MaybeGetAttribute(attributeName);
        if (attr == null)
        {
            string lineContents = GetStringLine(reader.LineNumber);

            throw new DrawIoException($"failed getting attribute `{attributeName}` from xml element `{reader.Name}`.\n" +
                $"Line contents: {lineContents}\n" +
                $"Location (usually relative to parent <mxGraphModel>): line {reader.LineNumber}, column {reader.LinePosition}.\n" +
                $"HELP INFO: https://github.com/StateSmith/StateSmith/issues/354");
        }
        return attr;
    }

    private string GetStringLine(int lineNumber)
    {
        string lineContents = StringUtils.ReadLineXFromString(lineNumber, xmlContents);
        lineContents = StringUtils.BackTickQuoteLimitedString(lineContents, 100);
        return lineContents;
    }

    private bool HasAttribute(string attributeName)
    {
        return reader.GetAttribute(attributeName) != null;
    }

    private bool HasAttributeValue(string attributeName, string value)
    {
        return reader.GetAttribute(attributeName)?.Equals(value) ?? false;
    }

    private string? MaybeGetAttribute(string attributeName)
    {
        return reader.GetAttribute(attributeName);
    }

    private double GetAttributeDoubleOrDefault(string attributeName, double default_value)
    {
        string value = MaybeGetAttribute(attributeName) ?? default_value + "";
        return double.Parse(value);
    }
}
