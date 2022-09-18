# Intro
Use this project with [quickstart.md](../../docs/quickstart1/quick-start.md).

It only covers some of the basics of StateSmith. Other examples explore more capabilities.

Directory `src` contains the C code to compile and run.

Directory `CodeGen` contains the C# user project that runs StateSmith and generates the state machine .c/.h files.


# Tutorial assumptions

While the state machine code generation is cross platform (see below), this tutorial project assumes that you are using `gcc` for compiling the c code. It should compile fine using MSVC too, but I haven't tested it yet. If you are on Windows, I would highly recommend taking a look at [WSL2](https://learn.microsoft.com/en-us/windows/wsl/install) - it's amazing!!! I actually use WSL2 for this tutorial.

Nothing about the state machine generated code requires Linux, but the demo code that runs the state machine uses some VT100 commands. You can compile the code using MSVC, but I'm not sure how the VT100 commands will work in a regular windows terminal.


# Building

## C# Code generation scripts
The c# code generation project will work on any platform if you have the [dotnet 6 sdk installed](https://dotnet.microsoft.com/en-us/download/dotnet/sdk-for-vs-code).

## Using command line only
- Linux/mac: cd to `src` and run `./compile_and_run.sh`. This will generate the state machine code, compile it, and run it.
- Windows: cd to `src` and run `bat-run-code-gen.bat`. This will generate the state machine code. MSVC compiling of the c code hasn't been added to the examples yet.

## vscode
If you use vscode, there are pre-configured `launch.json` and `tasks.json` files to help run and debug the c and c# programs.

I noticed that vscode intellisense can sometimes get confused if opened in this root directory as it sees C# and C projects.
If you encounter that problem too, you can open vscode in `src` for building/debugging the C code and open another vscode in `CodeGen` for the code gen.

