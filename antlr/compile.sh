#!/bin/bash -e

jarfile=antlr-4.9.2-complete.jar
libdir=./lib
jarpath=$libdir/$jarfile

export CLASSPATH=".:./out:./$libdir/$jarfile"

if [ ! -f $jarpath ]; then
    mkdir -p $libdir
    wget -O $jarpath https://www.antlr.org/download/$jarfile 
fi

# plantuml grammar
java -jar ./lib/$jarfile PlantUML.g4 -o out
java -jar ./lib/$jarfile PlantUML.g4 -Dlanguage=CSharp -visitor -o ../src/StateSmith/Input/PlantUML/antlr4/

# statesmith grammar
java -jar ./lib/$jarfile StateSmithLabelGrammar.g4 -o out
java -jar ./lib/$jarfile StateSmithLabelGrammar.g4 -Dlanguage=CSharp -visitor -o ../src/StateSmith/Input/antlr4/

javac out/*.java

