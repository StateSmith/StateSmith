#nullable enable

using StateSmith.Output;
using System.Text.RegularExpressions;

namespace StateSmith.SmGraph;

public class BehaviorDescriber
{
    readonly bool _singleLineFormat;
    readonly string indent = "";

    public BehaviorDescriber(bool singleLineFormat = false, string indent = "")
    {
        _singleLineFormat = singleLineFormat;
        this.indent = indent;
    }

    public string Describe(Behavior b)
    {
        string result = "";
        string joiner = "";

        if (b.order == Behavior.ELSE_ORDER)
        {
            result += joiner + "else";
            joiner = " ";
        }
        else if (b.order != Behavior.DEFAULT_ORDER)
        {
            result += joiner + b.order + ".";
            joiner = " ";
        }

        if (b.HasAtLeastOneTrigger())
        {
            string prefix = "", postfix = "";
            if (b.Triggers.Count > 1)
            {
                prefix = "(";
                postfix = ")";
            }

            result += prefix + joiner + string.Join(", ", b.Triggers) + postfix;
            joiner = " ";
        }

        if (b.HasGuardCode())
        {
            result += joiner + "[" + GetGuardCode(b) + "]";
            joiner = " ";
        }

        if (b.HasActionCode())
        {
            result += joiner + "/ { " + GetActionCode(b) + " }";
            joiner = " ";
        }

        if (b.HasViaExit())
        {
            result += joiner + "via exit " + b.viaExit;
            joiner = " ";
        }

        if (b.HasViaEntry())
        {
            result += joiner + "via entry " + b.viaEntry;
            joiner = " ";
        }

        if (b.TransitionTarget != null)
        {
            result += joiner + "TransitionTo(" + Vertex.Describe(b.TransitionTarget) + ")";
        }

        if (indent != "")
        {
            result = StringUtils.Indent(result, indent);
        }

        return result;
    }

    private string GetActionCode(Behavior b)
    {
        if (_singleLineFormat)
        {
            return MakeSingleLineCode(b.actionCode);
        }
        else
        {
            return b.actionCode;
        }
    }

    private string GetGuardCode(Behavior b)
    {
        if (_singleLineFormat)
        {
            return MakeSingleLineCode(b.guardCode);
        }
        else
        {
            return b.guardCode;
        }
    }

    public static string MakeSingleLineCode(string str)
    {
        str = StringUtils.ReplaceNewLineChars(str.Trim(), @"\n");
        return str;
    }
}
