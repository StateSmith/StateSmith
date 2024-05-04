using CommandLine;

namespace StateSmith.Cli.Setup;

[Verb("setup", HelpText = "Helps setup stuff.")]
public class SetupOptions
{
    [Option("vscode-drawio-plugin", HelpText = VscodeSettingsUpdater.Description)]
    public bool VscodeDrawioPlugin { get; set; }
}
