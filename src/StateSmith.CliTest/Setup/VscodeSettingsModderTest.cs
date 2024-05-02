using FluentAssertions;
using StateSmith.Cli.Setup;
using StateSmithTest;
using Xunit;

namespace StateSmith.CliTest.Setup;

public class VscodeSettingsModderTest
{

    [Fact]
    public void Test1()
    {
        var modder = new VscodeSettingsModder();

        var input = """
            {
                "files.eol": "\n",
                "files.associations": {
                    "display.h": "c"
                }
            }
            """;
        
        string plugin = "${workspaceFolder}/.vscode/StateSmith-drawio-plugin-v0.6.0.js";

        string output = modder.AddHedietVscodePlugin(jsonStr: input, plugin: plugin);

        output.ShouldBeShowDiff("""
            {
              "files.eol": "\n",
              "files.associations": {
                "display.h": "c"
              },
              "hediet.vscode-drawio.plugins": [
                {
                  "file": "${workspaceFolder}/.vscode/StateSmith-drawio-plugin-v0.6.0.js"
                }
              ]
            }
            """, outputCleanActual: true);
    }
}
