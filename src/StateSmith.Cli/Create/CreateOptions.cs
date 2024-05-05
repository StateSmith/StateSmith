using CommandLine;

namespace StateSmith.Cli.Create;

[Verb("create", HelpText = Description)]
public class CreateOptions
{
    public const string Description = "Create a new StateSmith project from template.";
}
