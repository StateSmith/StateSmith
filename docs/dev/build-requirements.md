# Install dotnet SDK 9 (cross platform)
Cross platform [install instructions here](https://dotnet.microsoft.com/en-us/download/dotnet/9.0). Below info has addtional tips for a few OS.

### Ubuntu 24.04 [x64]

[quick snippet](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install?tabs=dotnet9&pivots=os-linux-ubuntu-2404):
```bash
sudo add-apt-repository ppa:dotnet/backports
sudo apt-get update && sudo apt-get install -y dotnet-sdk-9.0
```

### Windows [x64]
Direct [link](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-9.0.305-windows-x64-installer).


### Any OS/ARCH
Follow [instructions here](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).



<br>

# ✅ Validate `dotnet` is installed
Open a terminal and run command `dotnet --version`

You should see something like `9.0.110`
