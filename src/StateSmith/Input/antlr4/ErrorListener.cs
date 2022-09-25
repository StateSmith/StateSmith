using Antlr4.Runtime;
using System.Collections.Generic;
using System.IO;

namespace StateSmith.Input.antlr4
{
    public class ErrorListener : IAntlrErrorListener<IToken>
    {
        public List<Error> errors = new List<Error>();

        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            errors.Add(new Error() {
                offendingSymbol = offendingSymbol,
                line = line,
                column = charPositionInLine,
                message = msg,
                exception = e
            });
        }
    }
}
