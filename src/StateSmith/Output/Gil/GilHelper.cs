#nullable enable

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StateSmith.Output.C99BalancedCoder1;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace StateSmith.Output.Gil;

public class GilHelper
{
    public const string GilNoEmitPrefix = "____GilNoEmit_";
    public const string GilNoEmitEchoStringBoolFuncName = GilNoEmitPrefix + "echoStringBool";

    public static void AppendGilHelpersFuncs(OutputFile file)
    {
        file.AppendLine($"public bool {GilNoEmitEchoStringBoolFuncName}(string toEcho) {{ return true; }}");
    }

    public static string WrapRawCodeWithBoolReturn(string codeToWrap)
    {
        codeToWrap = Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(codeToWrap, quote: true);
        return $"{GilNoEmitEchoStringBoolFuncName}({codeToWrap})";
    }

    public static bool IsGilNoEmit(string identifierName)
    {
        return identifierName.StartsWith(GilNoEmitPrefix);
    }

    public static bool IsGilNoEmit(MethodDeclarationSyntax node)
    {
        return IsGilNoEmit(node.Identifier.ValueText);
    }

    public static void Compile(string programText, out CompilationUnitSyntax root, out SemanticModel model, OutputInfo? outputInfo = null)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
        root = tree.GetCompilationUnitRoot();
        ThrowOnError(tree.GetDiagnostics(), programText, outputInfo);

        var compilation = CSharpCompilation.Create("GilCompilation")
            .AddReferences(MetadataReference.CreateFromFile(
                typeof(string).Assembly.Location))
            .AddSyntaxTrees(tree);

        model = compilation.GetSemanticModel(tree);
        ThrowOnError(model.GetDiagnostics(), programText, outputInfo);
    }

    private static void ThrowOnError(IEnumerable<Diagnostic> enumerable, string programText, OutputInfo? outputInfo)
    {
        var errors = enumerable.Where(d => d.Severity == DiagnosticSeverity.Error);

        var message = "";

        foreach (var error in errors)
        {
            message += error.ToString() + "\n";
        }

        if (message.Length > 0 && outputInfo != null)
        {
            File.WriteAllText($"{outputInfo.outputDirectory}ErrorFile.gil.cs.txt", programText); // .txt so that it doesn't cause other compilation errors (if C# is targetted)
            throw new GilCompError(message);
        }
    }
}
