using FluentAssertions;
using StateSmith.Cli.Setup;
using StateSmithTest;
using Xunit;

namespace StateSmith.CliTest.Setup;

public class VscodeSettingsJsonModderTest
{
    VscodeSettingsJsonModder modder = new();
    string newPlugin = "${workspaceFolder}/.vscode/StateSmith-drawio-plugin-v0.6.0.js";
    string oldPlugin = "${workspaceFolder}/.vscode/StateSmith-drawio-plugin-v0.5.0.js";

    [Fact]
    public void AddIfMissing()
    {
        var input = """
            {
                "files.eol": "\n",
                "files.associations": {
                    "display.h": "c"
                }
            }
            """;

        string expected = """
            {
              "files.eol": "\n",
              "files.associations": {
                "display.h": "c"
              },
              "hediet.vscode-drawio.plugins": [
                {
                  "file": "<<NEW_PLUGIN_VALUE>>"
                }
              ]
            }
            """;

        Expect(input: input, expected: expected);
    }

    [Fact]
    public void AddIfEmpty()
    {
        var input = """
            {
                "files.eol": "\n",
                "files.associations": {
                    "display.h": "c"
                },
                "hediet.vscode-drawio.plugins": [

                ]
            }
            """;

        string expected = """
            {
              "files.eol": "\n",
              "files.associations": {
                "display.h": "c"
              },
              "hediet.vscode-drawio.plugins": [
                {
                  "file": "<<NEW_PLUGIN_VALUE>>"
                }
              ]
            }
            """;

        Expect(input: input, expected: expected);
    }

    [Fact]
    public void AddBesideOthers()
    {
        var input = """
            {
                "files.eol": "\n",
                "files.associations": {
                    "display.h": "c"
                },
                "hediet.vscode-drawio.plugins": [
                    {
                        "file": "some-other-plugin"
                    }
                ]
            }
            """;

        string expected = """
            {
              "files.eol": "\n",
              "files.associations": {
                "display.h": "c"
              },
              "hediet.vscode-drawio.plugins": [
                {
                  "file": "some-other-plugin"
                },
                {
                  "file": "<<NEW_PLUGIN_VALUE>>"
                }
              ]
            }
            """;

        Expect(input: input, expected: expected);
    }

    [Fact]
    public void AddBesideOldVersion()
    {
        var input = """
            {
                "files.eol": "\n",
                "files.associations": {
                    "display.h": "c"
                },
                "hediet.vscode-drawio.plugins": [
                    {
                      "file": "some-other-plugin"
                    },
                    {
                      "file": "<<OLD_PLUGIN_VALUE>>"
                    }
                ]
            }
            """;

        string expected = """
            {
              "files.eol": "\n",
              "files.associations": {
                "display.h": "c"
              },
              "hediet.vscode-drawio.plugins": [
                {
                  "file": "some-other-plugin"
                },
                {
                  "file": "<<OLD_PLUGIN_VALUE>>"
                },
                {
                  "file": "<<NEW_PLUGIN_VALUE>>"
                }
              ]
            }
            """;

        Expect(input: input, expected: expected);
    }

    [Fact]
    public void DontAddIfExists()
    {
        var input = """
            {
                "hediet.vscode-drawio.plugins": [
                    {
                        "file": "<<NEW_PLUGIN_VALUE>>"
                    }
                ],
                "files.eol": "\n",
                "files.associations": {
                    "display.h": "c"
                }
            }
            """;

        Expect(input: input, expected: null);
    }

    [Fact]
    public void DontAddIfExistsBesideOthers()
    {
        var input = """
            {
                "hediet.vscode-drawio.plugins": [
                    {
                        "file": "<<NEW_PLUGIN_VALUE>>"
                    },
                    {
                        "file": "some-other-plugin"
                    }
                ],
                "files.eol": "\n",
                "files.associations": {
                    "display.h": "c"
                }
            }
            """;

        Expect(input: input, expected: null);
    }

    private void Expect(string input, string? expected)
    {
        input = ReplaceSpecialValues(input);
        if (expected != null)
            expected = ReplaceSpecialValues(expected);

        string? output = modder.AddHedietVscodePlugin(jsonStr: input, plugin: newPlugin);

        if (expected == null)
        {
            output.Should().BeNull();
        }
        else
        {
            output.ShouldBeShowDiff(expected, outputCleanActual: true);
        }
    }

    private string ReplaceSpecialValues(string input)
    {
        input = input.Replace("<<NEW_PLUGIN_VALUE>>", newPlugin).Replace("<<OLD_PLUGIN_VALUE>>", oldPlugin);
        return input;
    }
}
