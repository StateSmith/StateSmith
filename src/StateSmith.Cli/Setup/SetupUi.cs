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

        // TODOLOW - colorize drawio file

        return 0;
    }

    private void SetupVscodeDrawioPlugin()
    {
        new VscodeSettingsUpdater(_console).Run();
    }

    private void SetupVscodeCsx()
    {
        new SetupVscodeCsxAction(_console).Run();
    }

    private void AskWhatToDo(string? title = null)
    {
        const string setupVscodeDrawio = VscodeSettingsUpdater.Description;
        const string setupVscodeCsx = "Set up vscode for C# script (csx) debugging & intellisense";
        string choice = _console.Prompt(new SelectionPrompt<string>()
            .Title(title ?? $"What did you want to do?")
            .AddChoices(new[] {
                setupVscodeDrawio,
                setupVscodeCsx,
            }));

        switch (choice)
        {
            case setupVscodeDrawio: SetupVscodeDrawioPlugin(); break;
            case setupVscodeCsx: SetupVscodeCsx(); break;
        }
    }
}
