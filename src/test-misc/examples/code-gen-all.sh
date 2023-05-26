#!/bin/bash

# This script is intended for maintainers of StateSmith. It allows them to update, 
# build and run all the code generation for the examples.

# Pass first argument set to 1 if you want to code gen based on local project instead of published nuget.
# Helpful when evaluating code gen changes during development.

# If this fails to run because a nuget package cannot be found, try this command: `dotnet nuget locals all --clear`

# exit when any command fails
set -e

USE_PROJECT_REF="${1:-0}"  # gets value of first arg to script or 0 if not set. https://stackoverflow.com/a/2013589/7331858
NUGET_VERSION=0.9.4-alpha-fix-1

# https://stackoverflow.com/questions/59895/how-do-i-get-the-directory-where-a-bash-script-is-located-from-within-the-script
EXAMPLES_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

sed_update_project_file() {
    sed -i -E "s#    (<PackageReference|<ProjectReference).*>#    ${1}#" CodeGen.csproj
}

set_project_to_use_nuget() {
    sed_update_project_file "<PackageReference Include=\"StateSmith\" Version=\"$NUGET_VERSION\" />"
}

set_project_to_use_local_csproj() {
    sed_update_project_file "<ProjectReference Include=\"../../../../StateSmith/StateSmith.csproj\" />"
}

update_and_run_project() {

    echo "-----------------------------------------------------------"
    echo $1

    cd $1 # $1 is first arg to function, not script

    if [ $USE_PROJECT_REF = "1" ]
    then
        set_project_to_use_local_csproj
    else
        set_project_to_use_nuget
    fi

    dotnet build
    dotnet ./bin/Debug/net6.0/CodeGen.dll

    # run again at end so that we effectively undo update to use StateSmith.csproj
    set_project_to_use_nuget
}

update_and_run_project "$EXAMPLES_DIR/ButtonSm1Cpp/CodeGen/"
update_and_run_project "$EXAMPLES_DIR/LaserTagMenu1/CodeGen/"
update_and_run_project "$EXAMPLES_DIR/Tutorial1-blank/CodeGen/"
update_and_run_project "$EXAMPLES_DIR/Tutorial1-complete/CodeGen/"
update_and_run_project "$EXAMPLES_DIR/BlankTemplate/CodeGen/"
update_and_run_project "$EXAMPLES_DIR/Blinky1/CodeGen/"
update_and_run_project "$EXAMPLES_DIR/Blinky1Printf/CodeGen/"
