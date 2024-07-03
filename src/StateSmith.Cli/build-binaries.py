#!/usr/bin/env python3

import os
import shutil
import glob

buildBinariesDir = "build-binaries-temp"
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

# ensure build directory exists
os.makedirs(buildBinariesDir, exist_ok=True)

# remove existing files in build directory
for f in glob.glob(f"{buildBinariesDir}/*"):
    os.remove(f)

# execute publish command for each RID
for rid in rids:
    cmd = f"""dotnet publish -c Release -r {rid} -p:PublishSingleFile=true --self-contained --framework {framework} /p:DefineConstants="SS_SINGLE_FILE_APPLICATION" """
    print()
    print(cmd)
    os.system(cmd)

    filename = "StateSmith.Cli"
    extension = ""
    if rid.startswith("win"):
        extension = ".exe"
    filename += extension

    # move file
    shutil.move(f"bin/Release/{framework}/{rid}/publish/{filename}", f"{buildBinariesDir}/ss.cli-{rid}{extension}")

    # remove publish directory to disk save space
    shutil.rmtree(f"bin/Release/{framework}/")