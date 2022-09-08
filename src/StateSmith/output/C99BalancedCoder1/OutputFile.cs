using System;
using System.Text;
using System.Text.RegularExpressions;

namespace StateSmith.output.C99BalancedCoder1
{
    public class OutputFile
    {
        readonly CodeGenContext ctx;
        readonly StringBuilder sb;
        private readonly CodeStyleSettings styler;
        int indentLevel = 0;

        public OutputFile(CodeGenContext codeGenContext, StringBuilder fileStringBuilder)
        {
            ctx = codeGenContext;
            sb = fileStringBuilder;
            styler = codeGenContext.style;
        }

        public void StartCodeBlock()
        {
            if (styler.BracesOnNewLines)
            {
                FinishLine();
                Append("{");
            }
            else
            {
                Append(" {");
            }

            FinishLine();
            indentLevel++;
        }

        /// <summary>
        /// Doesn't try to finish line first (depending on style settings).
        /// </summary>
        public void StartCodeBlockHere()
        {
            Append("{");
            FinishLine();
            indentLevel++;
        }

        public void FinishCodeBlock(string codeAfterBrace="")
        {
            indentLevel--;
            Append("}");
            sb.Append(codeAfterBrace); // this part shouldn't be indented

            if (styler.BracesOnNewLines)
            {
                if (indentLevel < 0)
                {
                    throw new InvalidOperationException("indent went negative");
                }
                FinishLine();
            }
        }

        public void AppendLines(string codeLines)
        {
            var lines = StringUtils.SplitIntoLines(codeLines);
            foreach (var line in lines)
            {
                AppendLine(line);
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

        public void AppendLine(string codeLine = "")
        {
            Append(codeLine);
            FinishLine();
        }

        public void AppendDetectNewlines(string code = "")
        {
            var lines = StringUtils.SplitIntoLines(code);
            foreach (var line in lines)
            {
                Append(line);
                if (lines.Length > 1)
                {
                    FinishLine();
                }
            }
        }

        public void Append(string code = "")
        {
            styler.Indent(sb, indentLevel);
            sb.Append(code);
        }

        public void FinishLine(string code = "")
        {
            sb.Append(code);
            sb.Append(styler.Newline);
        }

        public override string ToString()
        {
            return sb.ToString();
        }
    }
}
