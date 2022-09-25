# PlantUML input

All the features I've ever needed are supported (I used to generate PlantUML diagrams), but we can add more if needed.

https://plantuml.com/state-diagram

## Examples
- [blinky1_printf_sm.plantuml](../examples/Blinky1Printf/src/blinky1_printf_sm.plantuml)
- [ButtonSm1Cpp.puml](../examples/ButtonSm1Cpp/ButtonSm1Cpp/ButtonSm1Cpp.puml)

## Somewhat strict parsing
The PlantUML format is very flexible. To ensure that we import the diagram correctly, we throw a parse error
if we encounter something unexpected.

For example, if you try to use an inline color like `[*] -> LED_OFF #brown`, you'll get this error message:

```
StateSmith Runner - Compiling file: `src/blinky1_printf_sm.plantuml`
Exception thrown: 'Antlr4.Runtime.InputMismatchException' in Antlr4.Runtime.Standard.dll
line 10:15 mismatched input '#' expecting LINE_ENDER
Exception FormatException : PlantUML input failed parsing. Reason(s):
  - mismatched input '#' expecting LINE_ENDER at line 10 column 15. Offending symbol: `#`.
StateSmith Runner - finished with failure.
```

## Supported diagram elements:
- initial states
- simple/composite states
- transitions
- stereotypes
- notes (short/long/floating)

## Ignored diagram elements:
- `hide empty description`
- `scale *`
- `skinparam *`
- `skinparam <identifier> {*}`
- single line comments
- multiline line comments (allowed to be anywhere)

## Grammar
ANTLR4 grammar: [../antlr/PlantUML.g4](../antlr/PlantUML.g4)
