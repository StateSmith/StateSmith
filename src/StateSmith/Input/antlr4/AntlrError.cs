using Antlr4.Runtime;
using System.Collections.Generic;

#nullable enable

namespace StateSmith.Input.Antlr4;

public abstract class AntlrError
{
    public int line;
    public int column;
    public string message = "";
    public RecognitionException exception;

    public AntlrError(int line, int column, string message, RecognitionException exception)
    {
        this.line = line;
        this.column = column;
        this.message = message;
        this.exception = exception;
    }

    public abstract string BuildMessage();

    public static string ErrorsToReasonStrings(List<AntlrError> errors, string separator)
    {
        var reasons = "";
        var needsSeparator = false;
        foreach (var error in errors)
        {
            if (needsSeparator)
            {
                reasons += separator;
            }
            reasons += error.BuildMessage();
            needsSeparator = true;
        }

        return reasons;
    }
}
