# Install dotnet 9 SDK 9 [cross platform]
You need to download and install the dotnet 9 SDK because you will be compiling C#.

### Do you already have one?
Try running the command `dotnet --list-sdks`. If you see a 9.x.x, you can skip the rest of this page :)

```bash
$ dotnet --list-sdks
9.0.110 [/usr/lib/dotnet/sdk]
```

### Windows
Direct [x64 installer link](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-9.0.305-windows-x64-installer).

When you have to [run specification tests](docs\devs\lib-test-specifications.md), it is recommended to use WSL2 with the Ubuntu instructions below.

### Ubuntu
Here's a quick snippet for Ubuntu [x64 24.04/22.04](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install?tabs=dotnet9&pivots=os-linux-ubuntu-2404):
```bash
$ sudo add-apt-repository ppa:dotnet/backports
$ sudo apt-get update && sudo apt-get install -y dotnet-sdk-9.0
```

### macOS
Download [installer or binaries here](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).

### More OS & ARCH Links
If the above didn't apply/work, check [out this link here](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).

<br>

# ✅ Validate `dotnet` installation
Open a terminal and run command `dotnet --version`

You should see something like `9.0.110`