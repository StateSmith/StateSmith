This file enables debugging the current .csx file in vscode.

The [StateSmith.Cli tool](https://github.com/StateSmith/StateSmith/blob/main/src/StateSmith.Cli/README.md) will auto generate the improved `launch.json` file below for you, but it isn't smart enough yet to add it to an existing `launch.json` file. If you have an existing `launch.json` file, you will still need to manually add this:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "debug StateSmith code gen",
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
        },
    ]
}
```

Note that `dotnet-script init` will generate a suboptimal file for Windows users, but it can fixed easily with the above config. Details [here](https://github.com/dotnet-script/dotnet-script/issues/697#issuecomment-2094336137).
