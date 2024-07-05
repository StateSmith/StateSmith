#nullable enable

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.Output.Gil;

public class RoslynCompiler
{
    private readonly IRoslynMetadataProvider roslynMetadataProvider;

#if !SS_SINGLE_FILE_APPLICATION
    /// <summary>
    /// For testing purposes only.
    /// </summary>
    internal RoslynCompiler()
    {
        this.roslynMetadataProvider = new FileMetadataProvider();
    }
#endif

    public RoslynCompiler(IRoslynMetadataProvider roslynMetadataProvider)
    {
        this.roslynMetadataProvider = roslynMetadataProvider;
    }

    public void Compile(string gilCode, out CompilationUnitSyntax root, out SemanticModel model)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(gilCode);
        root = tree.GetCompilationUnitRoot();
        ThrowOnError(tree.GetDiagnostics(), gilCode);

        var compilation = CSharpCompilation.Create("GilCompilation")
            .AddReferences(roslynMetadataProvider.GetSystemReferences())
            .AddSyntaxTrees(tree);

        model = compilation.GetSemanticModel(tree);
        ThrowOnError(model.GetDiagnostics(), gilCode);

        static void ThrowOnError(IEnumerable<Diagnostic> enumerable, string programText)
        {
            var errors = enumerable.Where(d => d.Severity == DiagnosticSeverity.Error);

            var message = "";

            foreach (var error in errors)
            {
                message += error.ToString() + "\n";
            }

            if (message.Length > 0)
            {
                throw new TranspilerException(message, programText);
            }
        }
    }
}
