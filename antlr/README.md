StateSmith uses ANTLR4 grammar (.g4) files. There are lots of good tutorials and ebooks online.

To be perfectly honest, the StateSmith grammars are not perfect. Contributions welcome :)


<br>

# Requirements
Please use WSL2 or linux to run the scripts in this directory.

## WSL2 installation that allows GUI use
https://ubuntu.com/tutorials/install-ubuntu-on-wsl2-on-windows-11-with-gui-support#1-overview

## install java jdk for antlr4
`sudo apt install default-jdk`

If you want to uninstall java later, see https://stackoverflow.com/questions/23959615/how-to-remove-default-jre-java-installation-from-ubuntu



<br>

# Editing the grammar files
Highly recommend using vscode and this extension https://marketplace.visualstudio.com/items?itemName=mike-lischke.vscode-antlr4

![image](https://github.com/StateSmith/StateSmith/assets/274012/052549ca-7c50-4a0d-afdd-f138686b8441)

It gives highlighting, navigation and live error checking!




<br>

# Read and use the scripts in this directory
```
./install.sh

. setup.sh
. compile.sh
. test.sh
```
