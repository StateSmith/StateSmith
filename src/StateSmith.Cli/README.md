# StateSmith CLI
A massive step up in productivity. Create and setup new projects in seconds!

This tool is brand new (2024-05-06) and ready for early access. We've already fixed a few issues and it is working quite well right now. Please open an issue if you encounter any issues.

**Video Walkthough**: https://www.youtube.com/watch?v=2y1tLmNpz78

## Install
Ensure that you have a [dotnet sdk (version 6, 7 or 8)](https://dotnet.microsoft.com/en-us/download/dotnet/sdk-for-vs-code) installed. It's used for StateSmith as well.

Then install the CLI tool with the following command:
```
dotnet tool install --global StateSmith.Cli
```

`--global` will put the tool in a location that is in your PATH. This will allow you to run the tool with the `ss.cli` command from any directory.

![](./md-img/install.gif)

## Usage
### Create a new project
```
ss.cli create
```

This will bring up a wizard that guides you through quickly creating a new StateSmith project. It remembers your choices for the next time you run the command so you should only need to enter in the project name and then hit enter a few times to create a new project.

![](./md-img/create.gif)

### Run StateSmith code generation
Still a work in progress. For now, it will print a message saying it's not ready yet.

The idea is to make it super easy to run the code generation tool. It will be as simple as running the command `ss.cli run`. It will code generate for all StateSmith projects in a manifest file, or the current directory (recursive option). It also automatically skips a project if its generated code is already up to date.



<br>

## Updating
Issue the command
```
dotnet tool update --global StateSmith.Cli
```

![](./md-img/image.png)


## Uninstall
Issue the command
```
dotnet tool uninstall --global StateSmith.Cli
```

## Install Specific Version or Test Release
Test releases are usually unlisted on nuget website. They also aren't detected by ss.cli update checks.
```
dotnet tool install --global StateSmith.Cli --version 0.8.2-diag-only-1
```
