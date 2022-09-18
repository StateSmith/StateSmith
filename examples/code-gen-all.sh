#!/bin/bash

# This script is intended for maintainers of StateSmith. It allows them to update, 
# build and run all the code generation for the examples.

# exit when any command fails
set -e

NUGET_VERSION=0.5.4-alpha

# https://stackoverflow.com/questions/59895/how-do-i-get-the-directory-where-a-bash-script-is-located-from-within-the-script
EXAMPLES_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

update_and_run_project() {
    cd $1
    sed -i "s/\"StateSmith\" Version=\".*\"/\"StateSmith\" Version=\"$NUGET_VERSION\"/" CodeGen.csproj
    dotnet build
    dotnet ./bin/Debug/net6.0/CodeGen.dll
}

update_and_run_project "$EXAMPLES_DIR/BlankTemplate/CodeGen/"
update_and_run_project "$EXAMPLES_DIR/Blinky1/CodeGen/"
update_and_run_project "$EXAMPLES_DIR/Blinky1Printf/CodeGen/"
update_and_run_project "$EXAMPLES_DIR/ButtonSm1Cpp/CodeGen/"
update_and_run_project "$EXAMPLES_DIR/LaserTagMenu1/CodeGen/"
update_and_run_project "$EXAMPLES_DIR/Tutorial1-blank/CodeGen/"
update_and_run_project "$EXAMPLES_DIR/Tutorial1-complete/CodeGen/"
