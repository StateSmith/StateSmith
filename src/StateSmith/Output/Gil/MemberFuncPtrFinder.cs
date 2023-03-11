using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace StateSmith.Output.Gil;

/// <summary>
/// This class finds GIL methods that are used like function pointers.
/// </summary>
public class MemberFuncPtrFinder : CSharpSyntaxWalker
{
    private readonly CompilationUnitSyntax root;
    private readonly SemanticModel model;
    private readonly HashSet<MethodDeclarationSyntax> set = new();

    public MemberFuncPtrFinder(CompilationUnitSyntax root, SemanticModel model)
    {
        this.root = root;
        this.model = model;
    }

    // for now, we simply look for a method used in an assignment operation.
    public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        if (!node.IsKind(SyntaxKind.SimpleAssignmentExpression))
            return;

        if (node.Right is IdentifierNameSyntax)
        {
            var symbol = model.GetSymbolInfo(node.Right).Symbol;

            if (symbol is IMethodSymbol methodSymbol)
            {
                var methodDeclarationSyntax = (MethodDeclarationSyntax)methodSymbol.DeclaringSyntaxReferences.Single().GetSyntax();
                set.Add(methodDeclarationSyntax);
            }
        }
    }

    public HashSet<MethodDeclarationSyntax> Find()
    {
        set.Clear();
        Visit(root);
        return set;
    }
}
