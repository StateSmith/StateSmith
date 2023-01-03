
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

#nullable enable

namespace StateSmith.Input.DrawIo;

/// <summary>
/// Draw.io svg files embed special content that allows them to be edited losslessly in draw.io.
/// This class allows you to decode the special content and get to the regular draw.io design file xml.
/// 
/// More info: https://drawio-app.com/extracting-the-xml-from-mxfiles/
/// Useful testing tool: https://jgraph.github.io/drawio-tools/tools/convert.html
/// </summary>
public class DrawIoSvgDecoder
{
    /// <summary>
    /// Find all references and see unit test.
    /// </summary>
    /// <param name="textReader"></param>
    /// <returns></returns>
    public static string DecodeToOriginalDiagram(TextReader textReader)
    {
        string mxfileXml = GetMxfileFromSvg(textReader);

        var compressedContent = GetDiagramCompressedContents(mxfileXml);

        string actual = DecompressContent(compressedContent);

        return actual;
    }

    /// <summary>
    /// Find all references and see unit test.
    /// </summary>
    /// <param name="textReader"></param>
    /// <returns></returns>
    public static string DecodeFileToOriginalDiagram(string filePath)
    {
        try
        {
            return DecodeToOriginalDiagram(File.OpenText(filePath));

        }
        catch (Exception e)
        {
            throw new DrawIoException($"Failed decoding file {filePath}.", e);
        }
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
    /// example input: `<![CDATA[<mxfile><diagram id="Tqm6eFcu1KHT34LG2WWE" name="Page-1">exCompressedContents_fjfiweghglihwe...</diagram></mxfile>]]>`
    /// example output: `exCompressedContents_fjfiweghglihwe...`
    /// </summary>
    /// <param name="mxfileXml"></param>
    /// <returns></returns>
    /// <exception cref="DrawIoException"></exception>
    public static string GetDiagramCompressedContents(string mxfileXml)
    {
        using XmlTextReader reader = new(new StringReader(mxfileXml));
        reader.WhitespaceHandling = WhitespaceHandling.None;

        ReadAndExpectOpeningXmlTag(reader, "mxfile");
        ReadAndExpectOpeningXmlTag(reader, "diagram");
        var contents = reader.ReadInnerXml();  // advances past diagram closing tag
        MaybeThrowHelpfulMultipleDiagramMessage(reader);
        ExpectClosingXmlTag(reader, "mxfile"); // todo - support multiple diagrams per mxfile. https://github.com/StateSmith/StateSmith/issues/78

        return contents.Trim();
    }

    private static void MaybeThrowHelpfulMultipleDiagramMessage(XmlTextReader reader)
    {
        if (reader.LocalName == "diagram")
        {
            throw new DrawIoException($"draw.io files can only contain a single diagram/page for now. See https://github.com/StateSmith/StateSmith/issues/78 .");
        }
    }

    private static void ReadAndExpectOpeningXmlTag(XmlTextReader reader, string tag)
    {
        ReadOrThrow(reader, $"Expected opening xml tag `{tag}`");
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

    private static void ReadOrThrow(XmlTextReader reader, string expectedMessage)
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

        ReadAndExpectOpeningXmlTag(reader, "svg");

        var content = reader.GetAttribute("content");

        if (content == null)
        {
            throw new DrawIoException("Invalid draw.io file. Expected xml `content` attribute.");
        }

        return content;
    }
}