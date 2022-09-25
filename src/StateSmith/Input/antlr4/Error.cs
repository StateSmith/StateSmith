using Antlr4.Runtime;

namespace StateSmith.Input.antlr4
{
    public class Error
    {
        public IToken offendingSymbol;
        public int line;
        public int column;
        public string message;
        public RecognitionException exception;

        public string BuildMessage()
        {
            return $"{message} at line {line} column {column}. Offending symbol: `{offendingSymbol.Text}`";
        }
    }
}
