using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using System.IO;
using StateSmith.Output.UserConfig;
using StateSmith.Output.Algos.Balanced1;    // todo need a generic way of getting file name. RenderConfig?

#nullable enable

namespace StateSmith.Output.Gil.CSharp;

public class GilToCSharp : IGilTranspiler
{
    private readonly StringBuilder fileSb = new();
    private readonly RenderConfigCSharpVars renderConfigCSharp;
    private readonly RenderConfigVars renderConfig;
    private readonly OutputInfo outputInfo;
    private readonly NameMangler nameMangler;

    public GilToCSharp(OutputInfo outputInfo, NameMangler nameMangler, RenderConfigCSharpVars renderConfigCSharp, RenderConfigVars renderConfig)
    {
        this.outputInfo = outputInfo;
        this.nameMangler = nameMangler;
        this.renderConfigCSharp = renderConfigCSharp;
        this.renderConfig = renderConfig;
    }

    public void TranspileAndOutputCode(string programText)
    {
        //File.WriteAllText($"{outputInfo.outputDirectory}{nameMangler.SmName}.gil.cs", programText);

        // FIXME remove dirty hacks. Used for POC.
        programText = programText.Replace("383849285762 == 383849285762", "");
        programText = programText.Replace("public enum EventId", renderConfigCSharp.ClassCode + "\npublic enum EventId");

        programText = CSharpSyntaxTree.ParseText(programText).GetRoot().NormalizeWhitespace().SyntaxTree.GetText().ToString();

        fileSb.AppendLineIfNotBlank(renderConfig.FileTop);
        fileSb.AppendLineIfNotBlank(renderConfigCSharp.Usings);

        var nameSpace = renderConfigCSharp.NameSpace.Trim();

        if (nameSpace.Length > 0)
        {
            fileSb.AppendLine("namespace " + renderConfigCSharp.NameSpace);
            if (NameSpaceNeedsBraces(nameSpace))
            {
                fileSb.AppendLine("{");
            }
        }

        fileSb.Append(programText);

        if (NameSpaceNeedsBraces(nameSpace))
        {
            fileSb.AppendLine("}");
        }

        PostProcessor.PostProcess(fileSb);
        File.WriteAllText($"{outputInfo.outputDirectory}{nameMangler.BaseFileName}.cs", fileSb.ToString());
    }

    private static bool NameSpaceNeedsBraces(string nameSpace)
    {
        return !nameSpace.EndsWith(";");
    }
}
