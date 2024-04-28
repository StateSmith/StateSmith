using Spectre.Console;

namespace StateSmith.Cli.Run;

public class RunUi
{
    public RunHandler runHandler;
    IAnsiConsole _console = AnsiConsole.Console;


    public RunUi(RunHandler runHandler)
    {
        this.runHandler = runHandler;
    }

    public void HandleNoManifest()
    {
        const string autoCreateManifestFile = "Automatically create a manifest file. [grey]Recursively finds StateSmith csx files in this directory.[/]";
        const string blankManifest = "Create a blank manifest file here. [grey]You'll need to set it up. Good for huge directories.[/]";
        const string findAndRunCsxFilesRecursively = "Find and run StateSmith csx files [cyan]Recursively[/]. [grey]No manifest used or created.[/]";
        const string findAndRunCsxFiles = "Find and run StateSmith csx files here. [grey]No manifest used or created.[/]";
        const string searchUpForManifest = "Search up the directory tree for a manifest file and use that";
        string choice = _console.Prompt(new SelectionPrompt<string>()
            .Title("No statesmith.manifest.json found. What did you want to do?")
            .AddChoices(new[] {
                autoCreateManifestFile,
                blankManifest,
                findAndRunCsxFilesRecursively,
                findAndRunCsxFiles,
                searchUpForManifest,
            }));

        switch (choice)
        {
            case autoCreateManifestFile:
                runHandler.ScanAndCreateManifest();
                break;
            case blankManifest:
                runHandler.CreateBlankManifest();
                break;
            //case findAndRunCsxFiles:
            //    runHandler.Fin
            //    break;
            //case searchUpForManifest:
            //    runHandler.SearchUpForManifest();
            //    break;
        }
    }
}
