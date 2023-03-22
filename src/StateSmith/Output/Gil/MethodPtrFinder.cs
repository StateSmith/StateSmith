using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StateSmith.Common;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace StateSmith.Output.Gil;

/// <summary>
/// This class finds GIL methods that have their address taken.
/// </summary>
public class MethodPtrFinder : CSharpSyntaxWalker
{
    private readonly CompilationUnitSyntax root;
    private readonly SemanticModel model;
    
    public readonly HashSet<MethodDeclarationSyntax> methods = new();
    public readonly HashSet<IdentifierNameSyntax> identifiers = new();
    public DelegateDeclarationSyntax? delegateDeclarationSyntax;
    public INamedTypeSymbol? delegateSymbol;

    public MethodPtrFinder(CompilationUnitSyntax root, SemanticModel model)
    {
        this.root = root;
        this.model = model;
    }

    public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
    {
        if (delegateDeclarationSyntax != null)
            throw new TranspilerException("Assumes only a single delegate type at this point.");

        delegateDeclarationSyntax = node;
        delegateSymbol = model.GetDeclaredSymbol(node).ThrowIfNull();
    }

    // for now, we simply look for a method used in an assignment operation.
    public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        if (!node.IsKind(SyntaxKind.SimpleAssignmentExpression))
            return;

        var right = node.Right;

        if (right is MemberAccessExpressionSyntax accessExpressionSyntax)
        {
            if (GilTranspilerHelper.IsThisMethodAccess(accessExpressionSyntax, model))
            {
                right = accessExpressionSyntax.Name;
            }
        }

        if (right is IdentifierNameSyntax identifierNameSyntax)
        {
            MaybeAdd(identifierNameSyntax);
        }
    }

    public override void VisitArgument(ArgumentSyntax node)
    {
        var expression = node.Expression;

        if (expression.IsThisMemberAccess(out var memberAccess))
        {
            expression = memberAccess!.Name;
        }

        if (expression is IdentifierNameSyntax identifierNameSyntax)
        {
            MaybeAdd(identifierNameSyntax);
        }
    }

    private void MaybeAdd(IdentifierNameSyntax identifierNameSyntax)
    {
        var symbol = model.GetSymbolInfo(identifierNameSyntax).Symbol;

        if (symbol is IMethodSymbol methodSymbol)
        {
            var methodDeclarationSyntax = (MethodDeclarationSyntax)methodSymbol.DeclaringSyntaxReferences.Single().GetSyntax();
            methods.Add(methodDeclarationSyntax);
            identifiers.Add(identifierNameSyntax);
        }
    }

    public void Find()
    {
        methods.Clear();
        Visit(root);
    }
}
