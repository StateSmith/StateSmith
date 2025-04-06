# Install is Optional
You can use the [StateSmith.Cli](https://github.com/StateSmith/StateSmith/blob/main/src/StateSmith.Cli/README.md) without installing anything.

You only need to follow these instructions if you are using .csx files for advanced code gen.

<br><br><br>


# These instructions work for Windows, Linux, mac
Please report if you have any issues.

# Install `dotnet`
Type `dotnet --version` on the command line to see if you have the C# [dotnet 6 sdk (or newer) installed](https://dotnet.microsoft.com/en-us/download/dotnet/sdk-for-vs-code). Recommend installing the latest dotnet Long Term Support (LTS) SDK.

# Install `dotnet-script`
Install the `dotnet-script` tool with the below command:
```bash
dotnet tool install --global dotnet-script
```

More info on [c# scripts here](https://github.com/dotnet-script/dotnet-script) if you are interested.

# [optional] Install StateSmith.CLI
This new tool is becoming the recommended way to run StateSmith.

Install the `StateSmith.CLI` tool with the below command:
```bash
dotnet tool install --global StateSmith.CLI
```
More info on how to [update and use here](https://github.com/StateSmith/StateSmith/blob/main/src/StateSmith.Cli/README.md).