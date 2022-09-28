using Antlr4.Runtime;

namespace StateSmith.Input.antlr4
{
    public class Error
    {
        /// <summary>
        /// non-null for parse errors. null for lexer errors.
        /// </summary>
        public IToken? offendingSymbol;
        
        /// <summary>
        /// for lexer errors. doesn't seem to be useful. ignore for now.
        /// </summary>
        public int offendingChar;
        public int line;
        public int column;
        public string message;
        public RecognitionException exception;

        public string BuildMessage()
        {
            var msg = $"{message} at line {line} column {column}.";

            if (offendingSymbol != null)
            {
                msg += $" Offending symbol: `{offendingSymbol.Text}`."; // todolow would be nice to print lexer's token name https://stackoverflow.com/questions/4403878/antlr-get-token-name
            }

            return msg;
        }
    }
}
