using StateSmith.Cli.Setup;
using StateSmithTest;
using Xunit;

namespace StateSmith.CliTest.Setup;

public class LaunchJsonModderTest
{
    [Fact]
    public void ModAbsolutePaths()
    {
        var json = """
            {
                "version": "0.2.0",
                "configurations": [
                    {
                        "name": ".NET Script Debug",
                        "type": "coreclr",
                        "request": "launch",
                        "program": "dotnet",
                        "args": [
                            "exec",
                            "C:/Users/user/.dotnet/tools/.store/dotnet-script/1.5.0/dotnet-script/1.5.0/tools/net8.0/any/dotnet-script.dll",
                            "${file}"
                        ],
                        "cwd": "${workspaceRoot}",
                        "stopAtEntry": false
                    }
                ]
            }
            """;

        var newJson = LaunchJsonModder.MaybeMod(json);

        newJson.ShouldBeShowDiff("""
            {
                "version": "0.2.0",
                "configurations": [
                    {
                        "name": ".NET Script Debug",
                        "type": "coreclr",
                        "request": "launch",
                        "program": "${env:HOME}/.dotnet/tools/dotnet-script",
                        "args": [
                            "${file}"
                        ],
                        "windows": {
                            "program": "${env:USERPROFILE}/.dotnet/tools/dotnet-script.exe",
                        },
                        "logging": {
                            "moduleLoad": false
                        },
                        "cwd": "${workspaceRoot}",
                        "stopAtEntry": false
                    }
                ]
            }
            """, outputCleanActual: true);
    }

    [Fact]
    public void ModRelativeToAddLogging()
    {
        var json = """
            {
              "version": "0.2.0",
              "configurations": [
                {
                  "name": ".NET Script Debug",
                  "type": "coreclr",
                  "request": "launch",
                  "program": "${env:HOME}/.dotnet/tools/dotnet-script",
                  "args": ["${file}"],
                  "windows": {
                    "program": "${env:USERPROFILE}/.dotnet/tools/dotnet-script.exe",
                  },
                  "cwd": "${workspaceFolder}",
                  "stopAtEntry": false,
                }
              ]
            }
            """;

        var newJson = LaunchJsonModder.MaybeMod(json);

        newJson.ShouldBeShowDiff("""
            {
              "version": "0.2.0",
              "configurations": [
                {
                  "name": ".NET Script Debug",
                  "type": "coreclr",
                  "request": "launch",
                  "program": "${env:HOME}/.dotnet/tools/dotnet-script",
                  "args": [
                    "${file}"
                  ],
                  "windows": {
                    "program": "${env:USERPROFILE}/.dotnet/tools/dotnet-script.exe",
                  },
                  "logging": {
                    "moduleLoad": false
                  },
                  "cwd": "${workspaceRoot}",
                  "stopAtEntry": false,
                }
              ]
            }
            """, outputCleanActual: true);
    }
}
