using Spectre.Console;
using StateSmith.Cli.Utils;

namespace StateSmith.Cli.Setup;

public class SetupUi
{
    private SetupOptions opts;
    private IAnsiConsole _console;
    private readonly string _currentDirectory;

    public SetupUi(SetupOptions opts, IAnsiConsole console, string currentDirectory)
    {
        this.opts = opts;
        _console = console;
        this._currentDirectory = currentDirectory;
    }

    public int Run()
    {
        _console.MarkupLine("");
        UiHelper.AddSectionLeftHeader(_console, "Set Up");

        bool oneRun = false;

        if (opts.VscodeAll)
        {
            VscodeAll();
            oneRun = true;
        }

        if (opts.VscodeCsx)
        {
            SetupVscodeCsx();
            oneRun = true;
        }
        
        if (opts.VscodeDrawioPlugin)
        {
            SetupVscodeDrawioPlugin();
            oneRun = true;
        }
        
        if (!oneRun)
        {
            AskWhatToDo();
        }

        // TODOLOW - colorize drawio file

        return 0;
    }

    private void VscodeAll()
    {
        SetupVscodeDrawioPlugin();
        SetupVscodeCsx();
    }

    private void SetupVscodeDrawioPlugin()
    {
        new VscodeSettingsUpdater(_console).Run();
    }

    private void SetupVscodeCsx()
    {
        new SetupVscodeCsxAction(_console, verbose: opts.Verbose, currentDirectory: _currentDirectory).Run();
    }

    private void AskWhatToDo(string? title = null)
    {
        const string all = SetupOptions.SetupAllVscode;
        const string setupVscodeDrawio = VscodeSettingsUpdater.Description;
        const string setupVscodeCsx = SetupVscodeCsxAction.Description;
        string choice = _console.Prompt(new SelectionPrompt<string>()
            .Title(title ?? $"What did you want to set up?")
            .AddChoices(new[] {
                all,
                setupVscodeDrawio,
                setupVscodeCsx,
            }));

        switch (choice)
        {
            case all: VscodeAll(); break;
            case setupVscodeDrawio: SetupVscodeDrawioPlugin(); break;
            case setupVscodeCsx: SetupVscodeCsx(); break;
        }
    }
}
