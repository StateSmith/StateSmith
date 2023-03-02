#nullable enable

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace StateSmith.Output.Gil;

public class GilHelper
{
    public const string GilNoEmitPrefix = "____GilNoEmit_";
    public const string GilNoEmitEchoStringBoolFuncName = GilNoEmitPrefix + "echoStringBool";
    public const string GilAddessableFunction = GilNoEmitPrefix + "GilAddessableFunction";

    public static void AppendGilHelpersFuncs(OutputFile file)
    {
        file.AppendLine($"public static bool {GilNoEmitEchoStringBoolFuncName}(string toEcho) {{ return true; }}");
        file.AppendLine($"public class {GilAddessableFunction}<T> : System.Attribute where T : System.Delegate {{}}");
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

    public static bool HandleSpecialGilEmitClasses(ClassDeclarationSyntax classDeclarationSyntax, CSharpSyntaxWalker walker)
    {
        return classDeclarationSyntax.Identifier.Text == GilAddessableFunction;
    }

    public static bool HandleGilSpecialInvocations(InvocationExpressionSyntax node, StringBuilder sb)
    {
        bool gilEmitMethodFoundAndHandled = false;

        if (node.Expression is IdentifierNameSyntax ins)
        {
            if (ins.Identifier.Text == GilHelper.GilNoEmitEchoStringBoolFuncName)
            {
                gilEmitMethodFoundAndHandled = true;
                ArgumentSyntax argumentSyntax = node.ArgumentList.Arguments.Single();
                var unescaped = System.Text.RegularExpressions.Regex.Unescape(argumentSyntax.ToFullString());
                unescaped = unescaped[1..^1]; // range operator
                sb.Append(unescaped); // FIXME: this may not do everything we need. We need inverse of https://stackoverflow.com/a/58825732/7331858 
            }
        }

        return gilEmitMethodFoundAndHandled;
    }

    private static void ThrowOnError(IEnumerable<Diagnostic> enumerable, string programText, OutputInfo? outputInfo)
    {
        var errors = enumerable.Where(d => d.Severity == DiagnosticSeverity.Error
            // ignore errors caused by our GilAddessableFunction attribute
            && d.Id != "CS0404" // error CS0404: Cannot apply attribute class 'Spec2Sm.____GilNoEmit_GilAddessableFunction<T>' because it is generic
            && d.Id != "CS0698" // error CS0698: A generic type cannot derive from 'Attribute' because it is an attribute class
        ); 

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
