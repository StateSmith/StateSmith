using CommandLine;

namespace StateSmith.Cli.Setup;

[Verb("setup", HelpText = Description)]
public class SetupOptions
{
    public const string Description = "Set up vscode for StateSmith & csx files.";
    public const string SetupAllVscode = "vscode all.";

    [Option("vscode-all", HelpText = "Set up " + SetupAllVscode)]
    public bool VscodeAll { get; set; }

    [Option("vscode-drawio-plugin", HelpText = "Set up " + VscodeSettingsUpdater.Description)]
    public bool VscodeDrawioPlugin { get; set; }

    [Option("vscode-csx", HelpText = "Set up " + SetupVscodeCsxAction.Description)]
    public bool VscodeCsx { get; set; }

    [Option('v', "verbose", HelpText = "Enables verbose info printing.")]
    public bool Verbose { get; set; } = false;
}
