#!/usr/bin/env python3

import os
import shutil
import glob
import subprocess

framework = "net8.0"

# https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
rids = [
    "win-x64",
    "linux-x64",
    "osx-x64", # minimum macOS version is 10.12 Sierra

    "win-x86",
    "win-arm64",
    "osx-arm64",
    "linux-musl-x64",
    "linux-musl-arm64",
    "linux-arm",
    "linux-arm64",
]

# set working directory to the directory of this script
abspath = os.path.abspath(__file__)
dname = os.path.dirname(abspath)
os.chdir(dname)

def RunWindowsBinaryCommand(command):
    print(f"Running command: {command}")
    subprocess.run(f"ss.cli-win-x64.exe {command}", shell=True, check=True)

# remove existing files in build directory
for f in glob.glob(f"*"):
    if not f.endswith(".plantuml") and not f.endswith(".py"):
        os.remove(f)

# execute publish command for each RID
for rid in rids:
    cmd = f"""dotnet publish -c Release -r {rid} -p:PublishSingleFile=true --self-contained --framework {framework} /p:DefineConstants="SS_SINGLE_FILE_APPLICATION" """
    print()
    print(cmd)
    subprocess.run(cmd, cwd="..", shell=True, check=True)

    filename = "StateSmith.Cli"
    extension = ""
    if rid.startswith("win"):
        extension = ".exe"
    filename += extension

    dotnetBinDirectory =f"../bin/Release/{framework}/{rid}"

    # move file
    shutil.move(f"{dotnetBinDirectory}/publish/{filename}", f"ss.cli-{rid}{extension}")

    # remove publish directory to disk save space
    shutil.rmtree(f"{dotnetBinDirectory}")


# if on windows, try running the windows binary. Throw exception if it fails.
if os.name == 'nt':
    print()
    print("Testing built binary...")

    RunWindowsBinaryCommand("--version")
    RunWindowsBinaryCommand("version")
    RunWindowsBinaryCommand("run --here")

    input("Press Enter to continue...")
    print("cleaning up...")

    # remove generated files
    for f in glob.glob(f"*.[ch]"):
        os.remove(f)

    for f in glob.glob(f"*.html"):
        os.remove(f)

print("Done")
