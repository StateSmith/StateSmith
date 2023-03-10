using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

#nullable enable

namespace StateSmith.Output.Gil;

public class WalkableChildSyntaxList
{
    private readonly CSharpSyntaxWalker walker;
    private readonly ChildSyntaxList childSyntaxList;
    private int index = 0;

    public WalkableChildSyntaxList(CSharpSyntaxWalker walker, ChildSyntaxList childSyntaxList)
    {
        this.walker = walker;
        this.childSyntaxList = childSyntaxList;
    }

    public void VisitUpTo(Predicate<SyntaxNodeOrToken> test, bool including = false)
    {
        while (index < childSyntaxList.Count)
        {
            SyntaxNodeOrToken syntaxNodeOrToken = childSyntaxList[index];
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

    public void VisitNext(SyntaxNodeOrToken? syntaxNodeOrToken = null)
    {
        syntaxNodeOrToken ??= childSyntaxList[index];
        syntaxNodeOrToken.Value.VisitWith(walker);
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

    public void VisitRest()
    {
        while (index < childSyntaxList.Count)
        {
            SyntaxNodeOrToken syntaxNodeOrToken = childSyntaxList[index];
            VisitNext(syntaxNodeOrToken);
        }
    }
}
