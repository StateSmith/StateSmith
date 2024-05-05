using CommandLine;

namespace StateSmith.Cli.Setup;

[Verb("setup", HelpText = Description)]
public class SetupOptions
{
    public const string Description = "Set up vscode for StateSmith & csx files.";
    public const string SetupAllVscode = "Set up everything for vscode.";

    [Option("vscode-all", HelpText = SetupAllVscode)]
    public bool VscodeAll { get; set; }

    [Option("vscode-drawio-plugin", HelpText = VscodeSettingsUpdater.Description)]
    public bool VscodeDrawioPlugin { get; set; }

    [Option("vscode-csx", HelpText = SetupVscodeCsxAction.Description)]
    public bool VscodeCsx { get; set; }
}
