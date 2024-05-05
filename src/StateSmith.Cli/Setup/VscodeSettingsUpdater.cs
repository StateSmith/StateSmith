using Spectre.Console;
using StateSmith.Cli.Utils;

namespace StateSmith.Cli.Setup;

public class VscodeSettingsUpdater
{
    public const string Description = "vscode drawio extension with StateSmith plugin.";
    public const string stateSmithPluginFilePath = ".vscode/" + stateSmithFileName;

    const string settingsJsonPath = ".vscode/settings.json";
    const string stateSmithFileName = "StateSmith-drawio-plugin-v0.6.0.js";
    const string pluginJsonValue = "${workspaceFolder}/.vscode/" + stateSmithFileName;
    const string fileUrl = "https://github.com/StateSmith/StateSmith-drawio-plugin/releases/download/v0.6.0/" + stateSmithFileName;
    
    private IAnsiConsole _console;

    public VscodeSettingsUpdater(IAnsiConsole console)
    {
        this._console = console;
    }

    public void DownloadIfNeeded()
    {
        // check if file already exists
        if (System.IO.File.Exists(stateSmithPluginFilePath))
        {
            _console.MarkupLine($"[grey]No need to download StatSmith drawio plugin. File already exists:[/] {stateSmithPluginFilePath}");
        }
        else
        {
            // donwload file from fileUrl using HttpClient
            _console.Status()
                .AutoRefresh(true)
                .Spinner(Spinner.Known.Dots2)
                .Start("[yellow]Downloading file:[/] " + fileUrl, ctx =>
                {
                    ctx.Status("downloading...");

                    using (var client = new System.Net.Http.HttpClient())
                    {
                        var response = client.GetAsync(fileUrl).Result;
                        response.EnsureSuccessStatusCode();
                        byte[] content = response.Content.ReadAsByteArrayAsync().Result;
                        System.IO.File.WriteAllBytes(stateSmithPluginFilePath, content);
                    }
                });

            _console.MarkupLine($"[green]StateSmith plugin downloaded to:[/] {stateSmithPluginFilePath}");
        }
    }

    internal void Run()
    {
        UiHelper.AddSectionLeftHeader(_console, "Set up " + Description);

        //_console.WriteLine("\n");
        //_console.MarkupLine("[cyan]↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓[/]");
        //_console.MarkupLine("[cyan]>>> MAKE SURE you already have the vscode draw.io extension installed:       <<<[/]");
        //_console.MarkupLine("[cyan]>>> [u]https://marketplace.visualstudio.com/items?itemName=hediet.vscode-drawio[/] <<<[/]");
        //_console.MarkupLine("[cyan]↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑[/]");
        //_console.MarkupLine("[grey]This tool can't detect if you have it installed or not. Just a reminder.[/]");
        //_console.WriteLine("\n");

        ////sleep for 1 second to give user time to read
        //System.Threading.Thread.Sleep(1000);

        if (!System.IO.Directory.Exists(".vscode"))
        {
            _console.WriteLine("Creating .vscode directory.");
            System.IO.Directory.CreateDirectory(".vscode");
        }

        DownloadIfNeeded();
        ModifyVscodeSettingsFileIfNeeded();

        UiHelper.AddSectionLeftHeader(_console, "vscode drawio StateSmith setup complete", "green");
        _console.Markup("Tip! [cyan]Requires extension:[/] ");
        _console.MarkupLine("[blue][u]https://github.com/StateSmith/StateSmith-drawio-plugin/wiki/Use-with-vscode[/][/]");
        _console.Markup("Tip! More draw.io info : ");
        _console.MarkupLine("[blue][u]https://github.com/StateSmith/StateSmith/wiki/draw.io[/][/]");
    }

    private void ModifyVscodeSettingsFileIfNeeded()
    {
        // read .vscode/settings.json file if it exists
        string settingsJson = "{}";
        if (System.IO.File.Exists(settingsJsonPath))
        {
            _console.WriteLine($"Updating {settingsJsonPath} file for StateSmith plugin.");
            settingsJson = System.IO.File.ReadAllText(settingsJsonPath);
        }
        else
        {
            _console.WriteLine($"Creating {settingsJsonPath} file for StateSmith plugin.");
        }

        string? json = new VscodeSettingsJsonModder().AddHedietVscodePlugin(settingsJson, pluginJsonValue);

        if (json == null)
        {
            _console.MarkupLine("[grey]No changes needed to json file.[/]");
            return;
        }

        System.IO.File.WriteAllText(settingsJsonPath, json);
    }
}
