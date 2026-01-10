#!/usr/bin/env python3

### NOTE! See .github\workflows\release-ss.cli.yml for potentially updated build script/instructions.
### This script is more for manual testing and building. It may be outdated.


import os
import shutil
import glob
import subprocess

framework = "net9.0"

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

# remove existing files in build directory
for f in glob.glob(f"*"):
    if not f.endswith(".plantuml") and not f.endswith(".py"):
        os.remove(f)


# todo - make it work on all platforms
def run_windows_binary_command(command):
    print(f"Running command: {command}")
    subprocess.run(f"ss.cli-win-x64.exe {command}", shell=True, check=True)

def publish_for_rid(framework, rid, is_compressed):
    compressed_str = "-p:EnableCompressionInSingleFile=true" if is_compressed else ""
    cmd = f"""dotnet publish -c Release -r {rid} -p:PublishSingleFile=true --self-contained --framework {framework} {compressed_str} /p:DefineConstants="SS_SINGLE_FILE_APPLICATION" """
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
    if is_compressed:
        extension = "-compressed" + extension
    shutil.move(f"{dotnetBinDirectory}/publish/{filename}", f"ss.cli-{rid}{extension}")

    # remove publish directory to disk save space
    shutil.rmtree(f"{dotnetBinDirectory}")


# execute publish command for each RID
for rid in rids:
    publish_for_rid(framework, rid, is_compressed=False)
    publish_for_rid(framework, rid, is_compressed=True)

# if on windows, try running the windows binary. Throw exception if it fails.
if os.name == 'nt':
    print()
    print("Testing built binary...")

    run_windows_binary_command("--version")
    run_windows_binary_command("version")
    run_windows_binary_command("run --here --rebuild --verbose")

    input("Press Enter to continue...")
    print("cleaning up...")

    # remove generated files
    for f in glob.glob(f"*.[ch]"):
        os.remove(f)

    for f in glob.glob(f"*.html"):
        os.remove(f)

print("Done")
