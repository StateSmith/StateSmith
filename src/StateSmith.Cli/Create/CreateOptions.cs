using CommandLine;

namespace StateSmith.Cli.Create;

[Verb("create", HelpText = Description)]
public class CreateOptions
{
    public const string Description = "Create a new StateSmith project from template.";

    [Option("print-storage-paths", HelpText = "Shows the paths where data/settings is stored.")]
    public bool PrintDataSettingsPaths { get; set; }
}
