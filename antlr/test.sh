#!/bin/bash

# a helper file that runs some test input into ANTLR4 grun

# ensure to use setup.bash first
# call like this: . test.sh

# grun StateSmithLabelGrammar edge -gui -tokens -trace test_input.txt
grun PlantumlGrammar diagram -gui -tokens -trace puml_test_input_1.puml
# grun StateSmithLabelGrammar node -tokens -trace test_input.txt