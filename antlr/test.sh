#!/bin/bash

# a helper file that runs some test input into ANTLR4 grun

# ensure to use setup.bash first
# call like this: . test.sh

grun Grammar1 node -gui -tokens -trace test_input.txt
# grun Grammar1 node -tokens -trace test_input.txt