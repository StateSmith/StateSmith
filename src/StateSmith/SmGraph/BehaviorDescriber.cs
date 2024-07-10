#nullable enable

using StateSmith.Output;

namespace StateSmith.SmGraph;

public class BehaviorDescriber
{
    public bool describeTransition = true;

    readonly string _newLine;
    readonly bool _singleLineFormat;
    readonly string _indent = "";

    public BehaviorDescriber(bool singleLineFormat = false, string indent = "", string newLine = "\\n")
    {
        _singleLineFormat = singleLineFormat;
        _indent = indent;
        _newLine = newLine;
    }

    public string Describe(Behavior b, bool alwaysShowActionCode = false)
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

        if (alwaysShowActionCode || b.HasActionCode())
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

        if (describeTransition && b.TransitionTarget != null)
        {
            result += joiner + "TransitionTo(" + Vertex.Describe(b.TransitionTarget) + ")";
        }

        if (_indent != "")
        {
            result = StringUtils.Indent(result, _indent);
        }

        return result;
    }

    private string GetActionCode(Behavior b)
    {
        if (_singleLineFormat)
        {
            return MakeSingleLineCode(b.actionCode, newLine: _newLine);
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
            return MakeSingleLineCode(b.guardCode, newLine: _newLine);
        }
        else
        {
            return b.guardCode;
        }
    }

    public static string MakeSingleLineCode(string str, string newLine = "\\n")
    {
        str = StringUtils.ReplaceNewLineChars(str.Trim(), newLine);
        return str;
    }
}
