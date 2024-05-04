using Spectre.Console;
using StateSmith.Cli.Setup;
using System.IO;
using Xunit;

namespace StateSmith.CliTest.Setup;

public class VscodeSettingsUpdaterTest
{
    [Fact]
    public void TestDownload()
    {
        if (File.Exists(VscodeSettingsUpdater.stateSmithPluginFilePath))
            File.Delete(VscodeSettingsUpdater.stateSmithPluginFilePath);
        var vsu = new VscodeSettingsUpdater(AnsiConsole.Console);
        vsu.Run();
    }
}
