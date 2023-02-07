namespace StateSmith.Runner;

internal interface IConsolePrinter
{
    void OutputStageMessage(string message) => WriteLine("StateSmith Runner - " + message);
    void WriteErrorLine(string message);
    void WriteLine();
    void WriteLine(string message);
}
