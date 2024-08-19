#nullable enable

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace StateSmith.Input.DrawIo;

/// <summary>
/// Draw.io svg files embed special content that allows them to be edited losslessly in draw.io.
/// This class allows you to decode the special content and get to the regular draw.io design file xml.
/// 
/// More info: https://drawio-app.com/extracting-the-xml-from-mxfiles/
/// Useful testing tool: https://jgraph.github.io/drawio-tools/tools/convert.html
/// </summary>
public class DrawIoDecoder
{
    /// <summary>
    /// Find all references and see unit test.
    /// </summary>
    /// <param name="textReader"></param>
    /// <returns></returns>
    public static List<DrawIoDiagramNode> DecodeSvgToOriginalDiagrams(TextReader textReader)
    {
        string mxfileXml = GetMxfileFromSvg(textReader);

        return GetMxFileDiagramContents(mxfileXml);
    }

    /// <summary>
    /// Decompresses diagram contents if required. 
    /// The vscode extension tends to write diagram contents uncompressed, 
    /// but the draw.io windows app tends to write it compressed.
    /// We have to be able to handle either.
    /// </summary>
    /// <param name="mxfileXml"></param>
    /// <returns></returns>
    public static List<DrawIoDiagramNode> GetMxFileDiagramContents(string mxfileXml)
    {
        var diagrams = GetDiagramsContentsRaw(mxfileXml);

        foreach (var diagram in diagrams)
        {
            if (!IsDiagramContentUncompressed(diagram.xml))
            {
                diagram.xml = DecompressContent(diagram.xml);
            }
        }

        return diagrams;
    }

    private static bool IsDiagramContentUncompressed(string diagramContents)
    {
        return Regex.IsMatch(diagramContents, @"\s*<mxGraphModel");
    }

    /// <summary>
    /// Find all references and see unit test.
    /// </summary>
    /// <param name="compressedContent"></param>
    /// <returns></returns>
    public static string DecompressContent(string compressedContent)
    {
        byte[] data = Convert.FromBase64String(compressedContent);

        using var compressedFileStream = new MemoryStream(data);
        using var decompressor = new DeflateStream(compressedFileStream, CompressionMode.Decompress);

        using var output = new MemoryStream();
        decompressor.CopyTo(output);

        string result = Encoding.UTF8.GetString(output.GetBuffer());
        result = Uri.UnescapeDataString(result);
        return result;
    }

    /// <summary>
    /// Returns whatever is inside  <![CDATA[<mxfile><diagram></diagram></mxfile>]]>. Might be compressed content. Might be regular xml.
    /// example input: `<![CDATA[<mxfile><diagram id="Tqm6eFcu1KHT34LG2WWE" name="Page-1">exCompressedContents_fjfiweghglihwe...</diagram></mxfile>]]>`
    /// example output: `exCompressedContents_fjfiweghglihwe...`
    /// </summary>
    /// <param name="mxfileXmlContents"></param>
    /// <returns></returns>
    /// <exception cref="DrawIoException"></exception>
    public static List<DrawIoDiagramNode> GetDiagramsContentsRaw(string mxfileXmlContents)
    {
        List<DrawIoDiagramNode> diagrams = new();
        using XmlTextReader reader = new(new StringReader(mxfileXmlContents));

        // keep whitespace so that ReadInnerXml contains whitespaces (required for error line numbers)
        reader.WhitespaceHandling = WhitespaceHandling.All;

        ReadOrThrow(reader, "Expecting <mxfile> tag");
        ExpectOpeningXmlTag(reader, "mxfile");

        ReadOrThrow(reader, "Expecting <diagram> tag");

        while (true)
        {
            diagrams.Add(ReadDiagramNode(reader));
            SkipWhiteSpaceToNextElement(reader);

            if (reader.LocalName != "diagram")
            {
                break;
            }
        }

        ExpectClosingXmlTag(reader, "mxfile");

        return diagrams;
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/78
    /// </summary>
    private static DrawIoDiagramNode ReadDiagramNode(XmlTextReader reader)
    {
        DrawIoDiagramNode diagramNode = new();
        ExpectOpeningXmlTag(reader, "diagram");
        diagramNode.id = reader.GetAttribute("id") ?? "";
        diagramNode.name = reader.GetAttribute("name") ?? "";

        SkipWhiteSpaceToNextElement(reader);     // skip whitespace to improve error messages
        diagramNode.xml = reader.ReadInnerXml();  // advances past diagram closing tag
        diagramNode.xml = diagramNode.xml.Trim();
        return diagramNode;
    }

    private static void ExpectOpeningXmlTag(XmlTextReader reader, string tag)
    {
        if (!reader.IsStartElement(tag))
        {
            throw new DrawIoException($"Invalid draw.io file. Expected opening xml tag `{tag}`, but found `{reader.LocalName}`");
        }
    }

    private static void ExpectClosingXmlTag(XmlTextReader reader, string tag)
    {
        if (reader.NodeType != XmlNodeType.EndElement)
        {
            throw new DrawIoException($"Invalid draw.io file. Expected closing xml tag `{tag}`, but found `{reader.LocalName}`");
        }
    }

    private static void SkipWhiteSpaceToNextElement(XmlTextReader reader, string? errorMessage = null)
    {
        if (reader.NodeType == XmlNodeType.Whitespace)
        {
            ReadOrThrow(reader, errorMessage ?? "Expected non-whitespace element");
        }
    }

    private static void ReadOrThrow(XmlTextReader reader, string expectedMessage)
    {
        while (true)
        {
            ReadOrThrowRaw(reader, expectedMessage);

            // skip whitespace
            if (reader.NodeType != XmlNodeType.Whitespace)
            {
                break;
            }
        }
    }

    private static void ReadOrThrowRaw(XmlTextReader reader, string expectedMessage)
    {
        if (!reader.Read())
        {
            throw new DrawIoException($"Invalid draw.io file. {expectedMessage}, but found none.");
        }
    }

    /// <summary>
    /// returns something like this: <![CDATA[<mxfile><diagram id="Tqm6eFcu1KHT34LG2WWE" name="Page-1"> compressed inner stuff </diagram></mxfile>]]>
    /// </summary>
    /// <param name="textReader"></param>
    /// <returns></returns>
    /// <exception cref="DrawIoException"></exception>
    private static string GetMxfileFromSvg(TextReader textReader)
    {
        using XmlTextReader reader = new(textReader);

        ExpectOpeningXmlTag(reader, "svg");

        var content = reader.GetAttribute("content");

        if (content == null)
        {
            throw new DrawIoException("Invalid draw.io file. Expected xml `content` attribute.");
        }

        return content;
    }
}
