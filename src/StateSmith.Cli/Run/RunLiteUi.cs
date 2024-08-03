using Spectre.Console;
using StateSmith.Cli.Utils;

namespace StateSmith.Cli.Run;

// RunLite is an alternative to RunUi that is more lightweight.
// - It outsources file and directory scanning to the shell. Users pass in the files
//   they want to run rather than having the CLI scan for them.
// - It is stateless. It does not save state to runinfo dbs or manifest files.
//   This significantly reduces complexity, but it still provides the ability to
//   watch for changes and re-run changed files.
// 
// Usage examples:
//   Transform all specified files (diagrams or csx) in the current directory and subdirectories
//       zsh> StateSmith.Cli --lang=JavaScript **/*.drawio.svg
//       bash> StateSmith.Cli --lang=JavaScript **/*.csx  # assumes shopt -s globstar
//       cmd.exe> StateSmith.Cli --lang=JavaScript *.drawio.svg   # current directory only # TODO verify
//       PowerShell> Get-ChildItem -Path .\*.drawio.svg -Recurse | ForEach StateSmith.Cli --lang=JavaScript # TODO verify and avoid foreach


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
