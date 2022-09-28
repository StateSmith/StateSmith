#!/bin/bash

# a helper file that runs some test input into ANTLR4 grun

# ensure to use setup.bash first
# call like this: . test.sh

# grun StateSmithLabelGrammar edge -gui -tokens -trace ss_label_test_input.txt
grun StateSmithLabelGrammar edge -tokens -trace ss_label_test_input.txt
# grun StateSmithLabelGrammar node -gui -tokens -trace ss_label_test_input.txt
# grun StateSmithLabelGrammar node -tokens -trace ss_label_test_input.txt > /dev/null


# grun PlantUML diagram -gui -tokens -trace puml_test_input_1.puml
# if you are looking for an error, this will show only stderr
# grun PlantUML diagram -tokens -trace puml_test_input_1.puml > /dev/null