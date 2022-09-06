@echo off
:: REMEBER TO RUN IN CMD prompt and not POWERSHELL!

SET CLASSPATH=.;C:\libs\antlr-4.9.2-complete.jar;%CLASSPATH%

java org.antlr.v4.Tool %*
