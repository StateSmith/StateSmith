#nullable enable

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using StateSmith.Common;
using System;

namespace StateSmith.Output.Gil;

public class GilHelper
{
    protected const string GilNoEmitPrefix = "____GilNoEmit_";
    protected const string GilNoEmitEchoStringBoolFuncName = GilNoEmitPrefix + "echoStringBool";
    protected const string GilUnusedVarFuncName = GilNoEmitPrefix + "GilUnusedVar";

    /// <summary>
    /// This is a special attribute that is added to methods to mark them as "addressable". In C terms, it means
    /// that the address of this function will be taken. Languages that don't support naked function pointers will
    /// need to convert these GIL methods into static readonly lambdas. This attribute takes a generic type with the
    /// recommended delegate type. Generic attributes is coming in C#11, so for now we have to ignore roslyn compilation
    /// errors specific to it.
    /// 
    /// See https://github.com/StateSmith/StateSmith/wiki/Multiple-Language-Support#function-pointers
    /// </summary>
    public const string GilAddessableFunctionAttribute = GilNoEmitPrefix + "GilAddessableFunction";


    public static void AppendGilHelpersFuncs(OutputFile file)
    {
        file.AppendLine($"public static bool {GilNoEmitEchoStringBoolFuncName}(string toEcho) {{ return true; }}");
        file.AppendLine($"public static void {GilUnusedVarFuncName}(object unusedVar) {{ }}");
        file.AppendLine($"public class {GilAddessableFunctionAttribute}<T> : System.Attribute where T : System.Delegate {{}}");
    }

    public static string MarkVarAsUnused(string varName)
    {
        return $"{GilUnusedVarFuncName}({varName});";
    }

    public static string WrapRawCodeWithBoolReturn(string codeToWrap)
    {
        codeToWrap = SymbolDisplay.FormatLiteral(codeToWrap, quote: true);
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

    public static void Compile(string programText, out CompilationUnitSyntax root, out SemanticModel model)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
        root = tree.GetCompilationUnitRoot();
        ThrowOnError(tree.GetDiagnostics(), programText);

        var compilation = CSharpCompilation.Create("GilCompilation")
            .AddReferences(MetadataReference.CreateFromFile(
                typeof(string).Assembly.Location))
            .AddSyntaxTrees(tree);

        model = compilation.GetSemanticModel(tree);
        ThrowOnError(model.GetDiagnostics(), programText);
    }

    public record AddressableFunctionInfo(INamedTypeSymbol DelegateSymbol, ParameterListSyntax ParameterListSyntax);

    /// <summary>
    /// See <see cref="GilAddessableFunctionAttribute"/>
    /// </summary>
    public static AddressableFunctionInfo? GetAddresssableFunctionInfo(MethodDeclarationSyntax node, SemanticModel semanticModel)
    {
        bool isAddressableFunction = false;
        foreach (var attributeList in node.AttributeLists)
        {
            if (attributeList.Attributes.Any(attr => attr.Name.ToString().StartsWith(GilHelper.GilAddessableFunctionAttribute))) // starts with because Name includes generic type. BlahBlahName<Func>
            {
                isAddressableFunction = true;
                break;
            }
        }

        if (!isAddressableFunction)
            return null;

        var attr = node.AttributeLists.Single().Attributes.Single();
        TypeSyntax delegateTypeSyntax = ((GenericNameSyntax)(attr.Name)).TypeArgumentList.Arguments.Single();
        var delegateSymbol = (semanticModel.GetSymbolInfo(delegateTypeSyntax).Symbol as INamedTypeSymbol).ThrowIfNull();

        var delegateDeclarationSyntax = (delegateSymbol.DeclaringSyntaxReferences[0].GetSyntax() as DelegateDeclarationSyntax).ThrowIfNull();

        return new(delegateSymbol, delegateDeclarationSyntax.ParameterList);
    }

    public static bool HandleSpecialGilEmitClasses(ClassDeclarationSyntax classDeclarationSyntax, CSharpSyntaxWalker walker)
    {
        return classDeclarationSyntax.Identifier.Text == GilAddessableFunctionAttribute;
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

    public static bool HandleGilUnusedVarSpecialInvocation(InvocationExpressionSyntax node, Action<ArgumentSyntax> codeBuilder)
    {
        bool gilMethodFoundAndHandled = false;

        if (node.Expression is IdentifierNameSyntax ins)
        {
            if (ins.Identifier.Text == GilHelper.GilUnusedVarFuncName)
            {
                gilMethodFoundAndHandled = true;
                ArgumentSyntax argumentSyntax = node.ArgumentList.Arguments.Single();
                codeBuilder(argumentSyntax);
            }
        }

        return gilMethodFoundAndHandled;
    }

    private static void ThrowOnError(IEnumerable<Diagnostic> enumerable, string programText)
    {
        var errors = enumerable.Where(d => d.Severity == DiagnosticSeverity.Error
            // ignore errors caused by our GilAddessableFunctionAttribute
            && d.Id != "CS0404" // error CS0404: Cannot apply attribute class 'Spec2Sm.____GilNoEmit_GilAddessableFunction<T>' because it is generic
            && d.Id != "CS0698" // error CS0698: A generic type cannot derive from 'Attribute' because it is an attribute class
        ); 

        var message = "";

        foreach (var error in errors)
        {
            message += error.ToString() + "\n";
        }

        if (message.Length > 0)
        {
            throw new TranspilerException(message);
        }
    }
}
