@echo off
call antlr4.bat Grammar1.g4
call antlr4.bat Grammar1.g4 -Dlanguage=CSharp -visitor -o ../StateSmith/StateSmith/Input/antlr4/

javac *.java
