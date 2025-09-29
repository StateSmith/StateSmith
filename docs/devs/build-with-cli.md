Follow [build-requirements.md](./build-requirements.md) first.

# Building via Command Line (any platform)
You can build StateSmith from the command line or with a GUI.

This page details how to build from the command line.

## Get the source code
git clone the StateSmith project

```shell
git clone https://github.com/StateSmith/StateSmith.git
```

or [download a zip](https://github.com/StateSmith/StateSmith/archive/refs/heads/main.zip) of the project.

## Build All StateSmith Projects
You can easily build all StateSmith C# projects with a single command.

The `src` directory contains 4 StateSmith C# Projects
* `src/StateSmith` - the core lib
* `src/StateSmithTest` - tests for the core lib
* `src/StateSmith.Cli` - the `ss.cli` CLI tool
* `src/StateSmith.CliTest` - tests for the `ss.cli` CLI tool

`cd` to the `src` directory and run command `dotnet build` to build. This works because there is a C# solution file (StateSmith.sln) in the `src` directory.

 You might need to run it twice for everything to get resolved. It should be good after that.

```shell
StateSmith/src$ dotnet build

Welcome to .NET 9.0!
---------------------
SDK Version: 9.0.110

<snip...>

Restore complete (11.4s)
  StateSmith succeeded with 1 warning(s) (2.9s) → StateSmith/bin/Debug/net6.0/StateSmith.dll
    /home/afk/code/StateSmith/src/StateSmith/Output/StringUtils.cs(160,33): warning CS8603: Possible null reference return.
  StateSmith.Cli net6.0 succeeded with 1 warning(s) (1.3s) → StateSmith.Cli/bin/Debug/net6.0/StateSmith.Cli.dll
  StateSmith.Cli net7.0 succeeded with 1 warning(s) (1.4s) → StateSmith.Cli/bin/Debug/net7.0/StateSmith.Cli.dll
  StateSmith.Cli net9.0 succeeded (2.7s) → StateSmith.Cli/bin/Debug/net9.0/StateSmith.Cli.dll
  StateSmith.Cli net8.0 succeeded (2.8s) → StateSmith.Cli/bin/Debug/net8.0/StateSmith.Cli.dll
  StateSmithTest succeeded (2.9s) → StateSmithTest/bin/Debug/net8.0/StateSmithTest.dll
  StateSmith.CliTest succeeded (0.7s) → StateSmith.CliTest/bin/Debug/net8.0/StateSmith.CliTest.dll

Build succeeded with 5 warning(s) in 18.1s
```

⚠️ Note: It's normal to see some warnings about net7.0 and net6.0 being out of support. We will eventually remove support for them in StateSmith (after giving a heads up to community via discord).

## Build again
Run command `dotnet build` to build.

It should complete much faster. Typically in a few seconds.


## Inject An Error And Build
Navigate to `src/StateSmith.Cli/Program.cs` and modify the Main method:

```c#
public static void Main(string[] args)
{
    int x = "this is an error. string assigned to int."; // ADD THIS!
    var program = new Program(currentDirectory: Environment.CurrentDirectory);
    program.Run(args);
}
```

Now use `dotnet build` again.

You should see some error output like:
```
StateSmith.Cli net9.0 failed with 1 error(s) (0.4s)
    StateSmith/src/StateSmith.Cli/Program.cs(36,17): error CS0029: Cannot implicitly convert type 'string' to 'int'
```

✅ Our dotnet compiler works.

Remove the error and get a clean build again.


# dotnet versions
You should be aware that the StateSmith lib and `ss.cli` tool support the old and unsupported `net6.0` and `net7.0` versions. We will eventually drop these versions after warning community via discord. Probably in 2026. Why keep them? Because early embedded developers adopted StateSmith and probably don't care to update their dotnet version.

Why does this matter? Well our production code can only use features available in dotnet6.0 (which corresponds to C#10).

However, the test projects will typically use the latest stable dotnet version so that we can make use of helpful features like [raw string literals](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/raw-string).

How can you ensure you don't accidentally use C#11+ features in production code? Easy! Just run `dotnet build` in `src` directory once in a while and it will check all versions.

Why even bring this up? Because as you'll see soon, you'll often be running `dotnet run --framework=net9.0` in the `src/StateSmith.Cli` directory. This will build all dependencies and run the project, but won't check for net6 compatibility in `ss.cli` code.

No worries if you forget as the github CI/CD will catch it.

<br><br>


# Build Tips

## Build Just One Project
There are 4 StateSmith C# Projects
* `src/StateSmith` - the core lib
* `src/StateSmithTest` - tests for the core lib
* `src/StateSmith.Cli` - the `ss.cli` CLI tool
* `src/StateSmith.CliTest` - tests for the `ss.cli` CLI tool

If you want to focus on just one, cd into that directory and run `dotnet build`




<!-- 

I think this below part can be deleted. No longer needed.

## Specify dotnet framework
A bit useful for `ss.cli` development.

cd to `src/StateSmith.Cli` and run `dotnet build`.

You should notice that it builds projects for net6.0/7.0/8.0/9.0. Why? Because `ss.cli` can be installed as a `dotnet tool`, but it needs to match the users dotnet version.

The downside to this is that you get 4 times the amount of build time and warnings.

You can simplify this by specifying a single framework to use.

```shell
src/StateSmith.Cli$ dotnet build --framework=net6.0
```

⚠️ NOTE! Use `net6.0` to ensure that your changes will work for all users and doesn't use features not available in net6.0.

-->
