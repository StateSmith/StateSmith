#!/bin/bash

# a helper file that runs some test input into ANTLR4 grun

# ensure to use setup.bash first
# call like this: . test.sh

# grun StateSmithLabelGrammar edge -gui -tokens -trace test_input.txt
grun PlantUML diagram -gui -tokens -trace puml_test_input_1.puml
# grun StateSmithLabelGrammar node -tokens -trace test_input.txt

# if you are looking for an error, this will show only stderr
# grun PlantUML diagram -tokens -trace puml_test_input_1.puml > /dev/null