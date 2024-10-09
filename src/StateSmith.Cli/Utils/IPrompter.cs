namespace StateSmith.Cli.Utils;

public interface IPrompter
{
    bool AskForOverwrite();
    bool YesNoPrompt(string title, string yesText = "yes");
    bool NoYesPrompt(string title, string noText = "no");
    T Prompt<T>(string title, UiItem<T>[] choices) where T : notnull;
}
