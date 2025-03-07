#!/bin/bash
gcc -std=c11 -Wall -Wno-unused-function -I ../../lang-helpers/c/ ../../lang-helpers/c/helper.c -o helper.o -c
g++ -std=c++11 -Wall -Wno-unused-function -I ../../lang-helpers/c/ main.cpp helper.o
