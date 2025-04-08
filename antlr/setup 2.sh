#!/bin/bash

# see README.md in this dir to help with install and setup

# call like this: . setup.sh
# see https://stackoverflow.com/questions/9772036/pass-all-variables-from-one-shell-script-to-another
export CLASSPATH=".:./out:./lib/antlr-4.9.2-complete.jar"
alias antlr4='java -jar ./lib/antlr-4.9.2-complete.jar'
alias grun='java org.antlr.v4.gui.TestRig'
