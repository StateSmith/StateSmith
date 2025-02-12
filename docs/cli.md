# Command Line Reference

```
statesmith [--flags] file1 [file2 ...]

Reads one or more state machine descriptions from the input file list
and generates code in the target language that implements that
state machine.

    --help   This message.
    --lang   One or more comma separate languages, eg. "--lang=js,ts". Defauts to 'js'
             js (JavaScript)
             ts (TypeScript)
             java
             c (alias to c99)
             c99
             cpp
             python
             csharp
             svg (image-only generation)

     -o      Output directory. Defaults to same directory as the source file.

     --tests Generate test scaffolding.      


```
