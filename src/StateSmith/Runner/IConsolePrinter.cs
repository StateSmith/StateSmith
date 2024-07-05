namespace StateSmith.Runner;

public interface IConsolePrinter
{
    void SetLineEnding(string lineEnding);
    void OutputStageMessage(string message) => WriteLine("StateSmith Runner - " + message);
    void WriteErrorLine(string message);
    void WriteLine();
    void WriteLine(string message);
}
