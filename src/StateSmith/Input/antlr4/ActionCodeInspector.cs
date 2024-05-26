using Antlr4.Runtime.Misc;
using System.Collections.Generic;

namespace StateSmith.Input.Antlr4;

public class ActionCodeInspector : StateSmithLabelGrammarBaseListener
{
    public HashSet<string> identifiersUsed = new();
    public HashSet<string> functionsCalled = new();

    public void Parse(string code)
    {
        var parser = new LabelParser();
        parser.ParseAndWalkAnyCode(this, code);
        if (parser.HasError())
        {
            //todolow improve error handling messages
            throw parser.GetErrors()[0].exception;
        }
    }

    public override void EnterExpandable_identifier([NotNull] StateSmithLabelGrammarParser.Expandable_identifierContext context)
    {
        string identifier = context.permissive_identifier().GetText();
        identifiersUsed.Add(identifier);
    }

    public override void EnterExpandable_function_call([NotNull] StateSmithLabelGrammarParser.Expandable_function_callContext context)
    {
        var functionName = context.permissive_identifier().GetText();
        functionsCalled.Add(functionName);
    }
}
