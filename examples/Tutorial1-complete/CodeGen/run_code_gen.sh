#!/bin/bash

# exit when any command fails
set -e

dotnet build ./CodeGen.csproj /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
dotnet ./bin/Debug/net6.0/CodeGen.dll