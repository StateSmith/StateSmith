using System;
using System.Text;

#nullable enable

namespace StateSmith.Output;

public class OutputFile
{
    private readonly StringBuilder sb;
    private readonly CodeStyleSettings styler;
    private int indentLevel = 0;
    private bool lineIncomplete = false;

    private bool lineBreakBeforeMoreCode = false;

    public OutputFile(CodeStyleSettings styler, StringBuilder fileStringBuilder)
    {
        sb = fileStringBuilder;
        this.styler = styler;
    }

    public void DecreaseIndentLevel()
    {
        indentLevel--;
    }

    public void IncreaseIndentLevel()
    {
        indentLevel++;
    }

    public void RequestNewLineBeforeMoreCode()
    {
        lineBreakBeforeMoreCode = true;
    }

    public OutputFileChangeTracker GetChangeTracker()
    {
        return new OutputFileChangeTracker(this);
    }

    public void StartCodeBlock(bool forceNewLine = false)
    {
        if (styler.BracesOnNewLines || forceNewLine)
        {
            MaybeFinishPartialLine();
            AppendIndented("{");
        }
        else
        {
            AppendWithoutIndent(" {");
        }

        FinishLine();
        indentLevel++;
    }

    /// <summary>
    /// Doesn't try to finish line first (depending on style settings).
    /// </summary>
    public void StartCodeBlockHere()
    {
        AppendIndented("{");
        FinishLine();
        indentLevel++;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="codeAfterBrace"></param>
    /// <param name="forceNewLine">probably the only time this should be false is when rendering in if/else</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void FinishCodeBlock(string codeAfterBrace = "", bool forceNewLine = true)
    {
        indentLevel--;
        if (indentLevel < 0)
        {
            throw new InvalidOperationException("indent went negative");
        }

        lineBreakBeforeMoreCode = false;

        AppendIndented("}");
        AppendWithoutIndent(codeAfterBrace); // this part shouldn't be indented

        if (styler.BracesOnNewLines || forceNewLine)
        {
            FinishLine();
        }
    }

    public void AppendLines(string codeLines, string prefix = "")
    {
        var lines = StringUtils.SplitIntoLinesOrEmpty(codeLines);
        foreach (var line in lines)
        {
            AppendIndentedLine(prefix + line);
        }
    }

    public void AppendLinesIfNotBlank(string code)
    {
        if (code.Length == 0)
        {
            return;
        }
        AppendLines(code);
    }

    public void AppendIndentedLine(string codeLine = "")
    {
        AppendIndented(codeLine);
        FinishLine();
    }

    //public void AppendDetectNewlines(string code = "")
    //{
    //    var lines = StringUtils.SplitIntoLinesOrEmpty(code);
    //    foreach (var line in lines)
    //    {
    //        AppendIndented(line);
    //        if (lines.Length > 1)   // TODO - this seems wrong. Shouldn't add extra trailing newline
    //        {
    //            FinishLine();
    //        }
    //    }
    //}

    public void AppendIndentNewlines(string code = "")
    {
        // code is done this way to so that lineIncomplete is set correctly

        var lines = StringUtils.SplitIntoLinesOrEmpty(code);
        bool needsIndent = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            AppendMaybeIndentLine(line, needsIndent);
            if (i < lines.Length - 1)
            {
                FinishLine();
                needsIndent = true;
            }
        }
    }

    public void AppendMaybeIndentLine(string code, bool indent)
    {
        if (indent)
        {
            AppendIndented(code);
        }
        else
        {
            AppendWithoutIndent(code);
        }
    }

    public void AppendWithoutIndent(string code = "")
    {
        if (code.Length == 0)
        {
            return;
        }

        lineIncomplete = true;
        sb.Append(code);
    }

    public void AppendIndented(string code = "")
    {
        if (lineBreakBeforeMoreCode)
        {
            lineBreakBeforeMoreCode = false;
            AppendIndentedLine();
        }

        styler.Indent(sb, indentLevel);
        AppendWithoutIndent(code);
    }

    public void FinishLine(string code = "")
    {
        AppendWithoutIndent(code);
        AppendWithoutIndent(styler.Newline);
        lineIncomplete = false;
    }

    public void MaybeFinishPartialLine(string code = "")
    {
        AppendWithoutIndent(code);

        if (lineIncomplete)
        {
            AppendWithoutIndent(styler.Newline);
        }
        lineIncomplete = false;
    }

    public override string ToString()
    {
        return sb.ToString();
    }

    public int GetStringBufferLength()
    {
        return sb.Length;
    }

    public string GetIndent()
    {
        return styler.Indent1;
    }
}
