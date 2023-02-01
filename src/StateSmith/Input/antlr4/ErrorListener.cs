using Antlr4.Runtime;
using System.Collections.Generic;
using System.IO;

namespace StateSmith.Input.Antlr4
{
    public class ErrorListener : IAntlrErrorListener<IToken> /* for parser errors */, IAntlrErrorListener<int> /* for lexer errors */
    {
        public List<AntlrError> errors = new();

        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            errors.Add(new ParseError(
                offendingSymbol: offendingSymbol,
                line: line,
                column: charPositionInLine,
                message: msg,
                exception: e
            ));
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingChar, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            errors.Add(new LexerError(
                offendingChar: offendingChar,
                line: line,
                column: charPositionInLine,
                message: msg,
                exception: e
            ));
        }
    }
}
