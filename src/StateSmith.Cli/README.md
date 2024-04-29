# StateSmith CLI
This tool is still a work in progress, but the project creation command is ready for early access. Please try it out and let me know what you think.

## Install
Ensure that you have a [dotnet sdk (ver 6+)](https://dotnet.microsoft.com/en-us/download/dotnet/sdk-for-vs-code) installed. It's used for StateSmith as well.

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
dotnet tool update StateSmith.Cli --global
```

![](./md-img/image.png)


## Uninstall
Issue the command
```
dotnet tool uninstall StateSmith.Cli --global
```
