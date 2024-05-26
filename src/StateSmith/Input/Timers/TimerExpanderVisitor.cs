#nullable enable

using Antlr4.Runtime.Tree;
using StateSmith.Input.Antlr4;
using StateSmith.Input.Expansions;
using StateSmith.Output.Gil;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace StateSmith.Input.Timers;

// FIXME - finish. This is mostly a copy paste from another expander visitor.
public class TimerExpanderVisitor : StateSmithLabelGrammarBaseVisitor<string>
{
    private readonly TimerExpander expander;

    public TimerExpanderVisitor(TimerExpander expander)
    {
        this.expander = expander;
    }

    public override string VisitTerminal(ITerminalNode node)
    {
        if (node.Symbol != null)
        {
            return node.Symbol.Text;
        }
        return "";
    }

    public override string VisitExpandable_identifier([NotNull] StateSmithLabelGrammarParser.Expandable_identifierContext context)
    {
        string result = context.ohs()?.GetText() ?? "";
        string identifier = context.permissive_identifier().GetText();
        identifier = expander.TryExpandVariableExpansion(identifier);
        result += identifier;

        return result;
    }

    public override string VisitExpandable_function_call([NotNull] StateSmithLabelGrammarParser.Expandable_function_callContext context)
    {
        var result = context.ohs()?.Accept(this) ?? "";

        var functionName = context.permissive_identifier().GetText();

        if (functionName == GilCreationHelper.GilExpansionMarkerFuncName)
        {
            string code = context.braced_function_args().function_args().GetText();
            result += WrappingExpander.HandleGilFunctionCode(code);
        }
        else if (expander.HasFunctionName(functionName))
        {
            result = ExpandFunctionCall(context, result, functionName);
        }
        else
        {
            result += functionName;
            result += context.braced_function_args().Accept(this);
        }

        return result;
    }

    private string ExpandFunctionCall(StateSmithLabelGrammarParser.Expandable_function_callContext context, string result, string functionName)
    {
        //We can't just visit the `function_args` rule because it includes commas and additional white space.

        var codeElements = context.braced_function_args().function_args()?.code_element();
        List<string> stringArgs = new();

        if (codeElements != null)
        {
            stringArgs.Add("");

            foreach (var item in codeElements)
            {
                if (item.code_line_element()?.code_symbol()?.COMMA() != null)
                {
                    stringArgs.Add("");
                }
                else
                {
                    stringArgs[^1] += item.Accept(this);
                }
            }
        }

        for (int i = 0; i < stringArgs.Count; i++)
        {
            stringArgs[i] = stringArgs[i].Trim();
        }

        var expandedCode = expander.TryExpandFunctionExpansion(functionName, stringArgs.ToArray());
        result += expandedCode;
        return result;
    }

    protected override string AggregateResult(string aggregate, string nextResult)
    {
        return aggregate + nextResult;
    }

    public string ParseAndExpandCode(string code)
    {
        var parser = new LabelParser();
        var result = parser.ParseAndVisitAnyCode(this, code);
        if (parser.HasError())
        {
            //todolow improve error handling messages
            throw parser.GetErrors()[0].exception;
        }
        return result;
    }
}
