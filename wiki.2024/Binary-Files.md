There are two main ways to run the StateSmith CLI:

1. Install the StateSmith CLI using the dotnet SDK.
    * Recommended if you already have a dotnet SDK (6,7,8) installed.
    * Smallest (28 MB), fastest, and simplest.
    * Instructions [here](https://github.com/StateSmith/StateSmith/blob/main/src/StateSmith.Cli/README.md).
1. Download the pre-built binary for your computer.
    * No need to install anything.
    * Doesn't require dotnet SDK.


<br>

# Standalone Binary Files
If you don't want to install anything, you can download standalone binary files for various platforms: https://github.com/StateSmith/StateSmith/releases


## Use Compressed Executable?
Compressed binary executables take a bit longer to launch (~70ms), but are less than 1/2 the file size.

> Note that the first run for compressed or uncompressed will take a few seconds. Subsequent runs are much faster.



## Platform Testing
NOTE! Only a few of the architectures have been tested as of July 2024.

Please report any tests (success/failure) [here](https://github.com/StateSmith/StateSmith/issues/260).

| arch             | compressed       | uncompressed | notes                     |
| ---------------- | ---------------- | ------------ | ------------------------- |
| win-x64          | ✅               | ✅           | Tested on WIN10/11        |
| linux-x64        | ✅               | ✅           | Tested using WSL2         |
| osx-x64          | ?                | ✅           | Works even for arm64 mac. NOTE1 |
| osx-arm64        | ? (probably not) | ❌           | Try using osx-x64 instead |
| win-x86          | ?                | ✅           | Tested on WIN10/11        |
| win-arm64        | ?                | ?            | Needs testing             |
| linux-musl-x64   | ?                | ?            | Needs testing             |
| linux-musl-arm64 | ?                | ?            | Needs testing             |
| linux-arm        | ?                | ?            | Needs testing             |
| linux-arm64      | ?                | ?            | Needs testing             |

* NOTE1 - minimum macOS version is 10.12 Sierra.


<br><br>

## Compressed Testing Notes
Running each program (uncompressed/compressed) a few times to "warm it up" as the very first run is pretty slow for both (around a few seconds).

## `win-x64`
* Uncompressed 145 MB takes about 160 ms to run `--version`.
* Compressed 60 MB takes about 230 ms `--version`.
