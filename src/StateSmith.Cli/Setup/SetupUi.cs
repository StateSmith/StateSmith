using Spectre.Console;
using System;
using System.IO;

namespace StateSmith.Cli.Setup;

public class SetupUi
{
    private SetupOptions opts;
    private IAnsiConsole _console;

    public SetupUi(SetupOptions opts, IAnsiConsole console)
    {
        this.opts = opts;
        _console = console;
    }

    public int Run()
    {
        if (opts.VscodeDrawioPlugin)
        {
            SetupVscodeDrawioPlugin();
        }
        else
        {
            AskWhatToDo();
        }

        // TODOLOW - help setup vscode script intellisense
        // TODOLOW - setup vscode with StateSmith plugin for draw.io extension
        // TODOLOW - colorize drawio file

        return 0;
    }

    private void SetupVscodeDrawioPlugin()
    {
        new VscodeSettingsUpdater(_console).Run();
    }

    private void SetupIntellisense()
    {
        // run command to setup intellisense
        // dotnet script init delete_me_dummy_file.csx

        // remove dummy file
        File.Delete("delete_me_dummy_file.csx");

        // provide help URL: https://github.com/StateSmith/StateSmith/wiki/Using-c%23-script-files-(.CSX)-instead-of-solutions-and-projects
    }

    private void AskWhatToDo(string? title = null)
    {
        const string setupVscodeDrawio = VscodeSettingsUpdater.Description;
        string choice = _console.Prompt(new SelectionPrompt<string>()
            .Title(title ?? $"What did you want to do?")
            .AddChoices(new[] {
                setupVscodeDrawio,
            }));

        switch (choice)
        {
            case setupVscodeDrawio:
                SetupVscodeDrawioPlugin();
                break;
        }
    }
}
