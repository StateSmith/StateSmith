using Spectre.Console;
using StateSmith.Cli.Utils;

namespace StateSmith.Cli.Run;

// RunLite is an alternative to RunUi that is more lightweight.
// - It outsources file and directory scanning to the shell. Users pass in the files
//   they want to run rather than having the CLI scan for them.
// - It is stateless. It does not save state to runinfo dbs or manifest files. 
//   This significantly reduces complexity.
public class RunLiteUi
{
    IAnsiConsole _console;
    RunLiteOptions opts;
    RunLiteHandler runLiteHandler;

    public RunLiteUi(RunLiteOptions opts, IAnsiConsole _console)
    {
        this.opts = opts;
        runLiteHandler = new(_console, opts.GetDiagramOptions(), opts);
        this._console = _console;
    }


    public int Run()
    {
        _console.MarkupLine("");
        UiHelper.AddSectionLeftHeader(_console, "RunLite");

        runLiteHandler.Run();
        return 0;
    }

}
