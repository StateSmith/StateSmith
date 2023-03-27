using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace StateSmith.Output.Gil;

public class WalkableChildSyntaxList
{
    private readonly CSharpSyntaxWalker walker;
    private readonly List<SyntaxNodeOrToken> nodeOrTokenList;
    private int index = 0;

    public WalkableChildSyntaxList(CSharpSyntaxWalker walker, ChildSyntaxList childSyntaxList)
    {
        this.walker = walker;
        this.nodeOrTokenList = childSyntaxList.ToList();
    }

    public WalkableChildSyntaxList(CSharpSyntaxWalker walker, SyntaxNode syntaxNode) : this(walker, syntaxNode.ChildNodesAndTokens())
    {

    }

    public void VisitUpTo(Predicate<SyntaxNodeOrToken> test, bool including = false)
    {
        while (index < nodeOrTokenList.Count)
        {
            SyntaxNodeOrToken syntaxNodeOrToken = nodeOrTokenList[index];
            if (test(syntaxNodeOrToken))
            {
                if (including)
                {
                    VisitNext(syntaxNodeOrToken);
                }
                return;
            }

            VisitNext(syntaxNodeOrToken);
        }
    }

    public void SkipUpTo(Predicate<SyntaxNodeOrToken> test, bool including = false)
    {
        while (index < nodeOrTokenList.Count)
        {
            SyntaxNodeOrToken syntaxNodeOrToken = nodeOrTokenList[index];
            if (test(syntaxNodeOrToken))
            {
                if (including)
                {
                    index++;
                }
                return;
            }

            index++;
        }
    }

    public void VisitNext(SyntaxNodeOrToken? syntaxNodeOrToken = null)
    {
        syntaxNodeOrToken ??= nodeOrTokenList[index];
        syntaxNodeOrToken.Value.VisitWith(walker);
        index++;
    }

    public void SkipNext()
    {
        index++;
    }

    public void VisitUpTo(SyntaxKind syntaxKind, bool including = false)
    {
        VisitUpTo((snot) => snot.IsKind(syntaxKind), including);
    }

    public void VisitUpTo(SyntaxToken syntaxToken, bool including = false)
    {
        VisitUpTo((snot) => snot == syntaxToken, including);
    }

    public void SkipUpTo(SyntaxToken syntaxToken, bool including = false)
    {
        SkipUpTo((snot) => snot == syntaxToken, including);
    }

    public void VisitUpTo(SyntaxNode syntaxNode, bool including = false)
    {
        VisitUpTo((snot) => snot == syntaxNode, including);
    }

    public bool TryRemove(SyntaxToken syntaxToken)
    {
        return nodeOrTokenList.Remove(syntaxToken);
    }

    public void Remove(SyntaxToken syntaxToken)
    {
        if (TryRemove(syntaxToken) == false)
            throw new ArgumentException("Failed to find syntaxToken to remove " + syntaxToken);
    }

    public void VisitRest()
    {
        while (index < nodeOrTokenList.Count)
        {
            SyntaxNodeOrToken syntaxNodeOrToken = nodeOrTokenList[index];
            VisitNext(syntaxNodeOrToken);
        }
    }
}
