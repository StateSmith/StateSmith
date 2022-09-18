#!/bin/bash

# exit when any command fails
set -e

# ensure to use setup.bash first
# call like this: . compile.sh
# see https://stackoverflow.com/questions/9772036/pass-all-variables-from-one-shell-script-to-another
antlr4 Grammar1.g4 -o out
antlr4 Grammar1.g4 -Dlanguage=CSharp -visitor -o ../src/StateSmith/Input/antlr4/

javac out/*.java
