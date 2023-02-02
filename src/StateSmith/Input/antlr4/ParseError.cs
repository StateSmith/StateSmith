using Antlr4.Runtime;

#nullable enable

namespace StateSmith.Input.Antlr4;

public class ParseError : AntlrError
{
    public IToken offendingSymbol;

    public ParseError(IToken offendingSymbol, int line, int column, string message, RecognitionException exception) : base(line, column, message, exception)
    {
        this.offendingSymbol = offendingSymbol;
    }

    public override string BuildMessage()
    {
        var msg = $"{message} at line {line} column {column}.";
        msg += $" Offending symbol: `{offendingSymbol.Text}`."; // todolow would be nice to print lexer's token name https://stackoverflow.com/questions/4403878/antlr-get-token-name

        return msg;
    }
}
