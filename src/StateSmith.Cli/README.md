# StateSmith CLI - a massive step up in productivity

🚀 Create new projects and generate code in seconds!

<br>

# Download Binary or Install
StateSmith is written in cross platform C# and can be run on Windows, Linux, and macOS. I chose C# because I make heavy use of the open source Roslyn compiler to transpile the generated state machine code into various programming languages.

There are two main ways to run the StateSmith CLI:
1. Download the [pre-built binary](https://github.com/StateSmith/StateSmith/releases) for your computer (no install needed, no dotnet sdk needed).
2. Install the StateSmith.CLI using the dotnet SDK.


<br>


## Install (simple, but requires dotnet SDK)
If you already have a dotnet SDK (6,7,8) installed, you can quickly install the StateSmith CLI using the following command. It is the smallest (28 MB), fastest, and simplest way to install the StateSmith CLI.

```sh
dotnet tool install --global StateSmith.Cli
```

`--global` will put the tool in a location that is in your PATH. This will allow you to run the tool with the `ss.cli` command from any directory. You can also install the tool locally to a specific project by omitting `--global`.


<br>


## Download the standalone binary (no dotnet required)
🔥 Please note that the standalone binaries are new. I've only been able to test on a few architectures [noted here](https://github.com/StateSmith/StateSmith/wiki/Binary-Files#platform-testing). Your help testing would be appreciated.

If you run into any issues, please consider installing the StateSmith CLI using a dotnet SDK as described in the previous section. It has worked well for many users across windows/linux/mac.

Binary release files: [https://github.com/StateSmith/StateSmith/releases](https://github.com/StateSmith/StateSmith/releases)

### Windows
1. Download the binary from our [releases page](https://github.com/StateSmith/StateSmith/releases). If unsure, use [windows-x64-compressed](https://github.com/StateSmith/StateSmith/releases/download/cli-v0.14.0/ss.cli-win-x64-compressed.exe).
1. Rename the downloaded binary to `ss.cli.exe` to be consistent with tutorials.
1. Consider putting the download binary somewhere that is on your PATH so you can run `ss.cli` from any directory.


### Linux
The below instructions assume Linux x64 (non-Alpine). Alpine and `ARM` binaries also [available](https://github.com/StateSmith/StateSmith/releases).

```sh
# download the 60 MB binary
wget https://github.com/StateSmith/StateSmith/releases/download/cli-v0.14.0/ss.cli-linux-x64-compressed

# rename the binary to be consistent with tutorials
mv ss.cli-linux-x64-compressed ss.cli

# make binary executable
chmod +x ss.cli

# optional: move the binary to a location in your PATH
# If you choose not to do this, you will need to run the binary with `./ss.cli`
sudo mv ss.cli /usr/local/bin
```

### Mac
🔥 We only have a bit of testing on macOS right now. It looks like `arm64` is not working, but you can use `x64` even on new arm based macs. See [comment here](https://github.com/StateSmith/StateSmith/issues/260#issuecomment-2210249795).


```sh
# download the 60 MB binary
wget https://github.com/StateSmith/StateSmith/releases/download/cli-v0.14.0/ss.cli-osx-x64-compressed

# rename the binary to be consistent with tutorials
mv ss.cli-osx-x64-compressed ss.cli

# make binary executable
chmod +x ss.cli

# optional: move the binary to a location in your PATH
# If you choose not to do this, you will need to run the binary with `./ss.cli`
sudo mv ss.cli /usr/local/bin
```

<br>


## Usage
### Create a new project
```
ss.cli create
```

This will bring up a wizard that guides you through quickly creating a new StateSmith project. It remembers your choices for the next time you run the command so you should only need to enter in the project name and then hit enter a few times to create a new project.

![](https://github.com/user-attachments/assets/7c00c5f5-baa3-4c25-94e4-53ac87069615)


<br>


### Run Code Generation
The `run` verb has numerous options.

Run code gen recursively in the current directory:
```sh
ss.cli run -hr
```

#### More options
```sh
ss.cli run --help
```

```
  -h, --here         Runs code generation in this directory.

  -b, --rebuild      Ensures code generation is run. Ignores change detection.

  -u, --up           Searches upwards for manifest file.

  -r, --recursive    Recursive. Can't use with -i.

  -x, --exclude      Glob patterns to exclude

  -i, --include      Glob patterns to include. ex: `**/src/*.csx`. Can't use
                     with -r.
<snip...>
```


<br>

### Version Info
```sh
ss.cli --version
```
You should see output similar to the following:

```
Using settings directory: /home/afk/.local/share/StateSmith.Cli
StateSmith.Cli 0.14.0+355742942da782ea2fa41b89efb45b9542e6cc79
```



<br>


### More Usage Info
You can run `ss.cli` by itself to bring up a menu of options. You can also run `ss.cli --help` to see a list of available verbs.


```sh
ss.cli --help
```

You should see output similar to the following:

```
Usage:

  run       Run StateSmith code generation.

  create    Create a new StateSmith project from template.
  
  setup     Set up vscode for StateSmith & csx files.

To get help for a specific verb, use the command name followed by --help
```



<br>
<br>


# `dotnet tool` Tips
If you installed the StateSmith CLI using the `dotnet tool` command, here's a number of helpful tips.

## Updating
```
dotnet tool update --global StateSmith.Cli
```

## Uninstall
```
dotnet tool uninstall --global StateSmith.Cli
```

## Install Specific Version or Test Release
Test releases are usually unlisted on nuget website. They also aren't detected by ss.cli update checks.
```
dotnet tool install --global StateSmith.Cli --allow-downgrade --version 0.8.2-diag-only-1
```


<br>
<br>

# More Info

## Manifest files [optional]
Here's an older video walkthough that is missing many new features, but it explains manifest files well.<br>
https://www.youtube.com/watch?v=2y1tLmNpz78




