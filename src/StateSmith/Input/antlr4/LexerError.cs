using Antlr4.Runtime;

#nullable enable

namespace StateSmith.Input.Antlr4;

public class LexerError : AntlrError
{
    public int offendingChar;

    public LexerError(int offendingChar, int line, int column, string message, RecognitionException exception) : base(line, column, message, exception)
    {
        this.offendingChar = offendingChar;
    }

    public override string BuildMessage()
    {
        var msg = $"{message} at line {line} column {column}.";
        msg += $" Offending char: `{offendingChar}`."; // todolow would be nice to print lexer's token name https://stackoverflow.com/questions/4403878/antlr-get-token-name

        return msg;
    }
}
