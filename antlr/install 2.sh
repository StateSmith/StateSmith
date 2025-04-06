#!/bin/bash

# exit when any command fails
set -e

jar_file=antlr-4.9.2-complete.jar

mkdir -p lib
cd lib
wget -O $jar_file https://www.antlr.org/download/$jar_file 
cd ..
