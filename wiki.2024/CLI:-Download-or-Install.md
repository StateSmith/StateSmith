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
1. Download the binary from our [releases page](https://github.com/StateSmith/StateSmith/releases). If unsure, use [windows-x64-compressed](https://github.com/StateSmith/StateSmith/releases/download/cli-v0.17.2/ss.cli-win-x64-compressed.exe).
1. Rename the downloaded binary to `ss.cli.exe` to be consistent with tutorials.
1. Consider putting the download binary somewhere that is on your PATH so you can run `ss.cli` from any directory.


### Linux
The below instructions assume Linux x64 (non-Alpine). Alpine and `ARM` binaries also [available](https://github.com/StateSmith/StateSmith/releases).

```sh
# download the 60 MB binary
wget https://github.com/StateSmith/StateSmith/releases/download/cli-v0.17.2/ss.cli-linux-x64-compressed

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
wget https://github.com/StateSmith/StateSmith/releases/download/cli-v0.17.2/ss.cli-osx-x64-compressed

# rename the binary to be consistent with tutorials
mv ss.cli-osx-x64-compressed ss.cli

# make binary executable
chmod +x ss.cli

# optional: move the binary to a location in your PATH
# If you choose not to do this, you will need to run the binary with `./ss.cli`
sudo mv ss.cli /usr/local/bin
```



<br>
<br>


## `dotnet tool` Tips
If you installed the StateSmith CLI using the `dotnet tool` command, here's a number of helpful tips.

### Updating
```
dotnet tool update --global StateSmith.Cli
```

### Uninstall
```
dotnet tool uninstall --global StateSmith.Cli
```

### Install Specific Version or Test Release
Test releases are usually unlisted on nuget website. They also aren't detected by ss.cli update checks.
```
dotnet tool install --global StateSmith.Cli --allow-downgrade --version 0.8.2-diag-only-1
```


<br>



## more info
See [CLI Wiki Root Page](https://github.com/StateSmith/StateSmith/wiki/CLI:-Command-Line-Interface)