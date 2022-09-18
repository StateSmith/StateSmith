@REM For windows users

@REM build user code generation program
dotnet build ../CodeGen/CodeGen.csproj /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary || exit /b

@REM run user code generation program
dotnet ../CodeGen/bin/Debug/net6.0/CodeGen.dll
