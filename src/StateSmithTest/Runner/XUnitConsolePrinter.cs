using Xunit.Abstractions;


namespace StateSmith.Runner;

public class XUnitConsolePrinter(ITestOutputHelper output) : IConsolePrinter
{
    private readonly ITestOutputHelper output = output;

    public void SetLineEnding(string lineEnding) {}

    public void WriteErrorLine(string message) => output.WriteLine(message ?? "");
    public void WriteLine(string message) => output.WriteLine(message ?? "");
    public void WriteLine() => output.WriteLine("");

}
