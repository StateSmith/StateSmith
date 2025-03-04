# Creating State Machines with PlantUML 

PlantUML is the preferred state machine language on the StateSmith documentation website, so you'll find many examples as you browse around the documentation.

<table>
<tr>
<td>
<img src="lightbulb.svg">
</td>
<td>
<pre>
{% include_relative lightbulb.puml %}
</pre>
</td>
</tr>
</table>

## (Optional) Install PlantUML

If you installed `StateSmith` the recommended way, PlantUML was included as a dependency on Mac and Linux. For Windows users, follow the installation instructions on [https://plantuml.com/](https://plantuml.com/) if you would like to generate images from your PlantUML files (recommended).

TODO verify dependency for Mac and Linux

## Generate images from `.puml` files

Generating an image is easy. You can easily choose between image formats such as SVG or PNG:

```
% plantuml -tsvg lightbulb.puml
% plantuml -tpng lightbulb.puml
```

## Generating source code

Using StateSmith to generate source code in the language of your choice is easy.

```
% statesmith lightbulb.puml
```

This will generate JavaScript by default (since most everyone can run JS conveniently in a browser). Visit the `Languages` section to see how to generate other supported languages.

## PlantUML state machine reference

The PlantUML website has a great deal of information about supported state machine syntax at: 
https://plantuml.com/state-diagram


## PlantUML editor

There is a handy PlantUML online editor with live preview at:
https://editor.plantuml.com/


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
- history states

## Ignored diagram elements:
- `hide empty description`
- `title *` (thanks [@yongzhy](https://github.com/StateSmith/StateSmith/issues/216))
- `scale *`
- `skinparam *`
- `skinparam <identifier> {*}`
- single line comments
- multiline line comments (allowed to be anywhere)


## State Behavior Across Multiple Lines
You can do either of the below:
```plantuml
OFF: enter / {
OFF: \tcount = 0;
OFF: \tprint("OFF");
OFF: }
```
```plantuml
OFF: enter / {\n    count = 0;\n    print("OFF");\n}
```

You'll need to use `\n` syntax for multiple line transition code for now though. If you do that, see other tip here about left aligning transition labels.


## Layout Tips

#### Left Align Transition Label
https://github.com/StateSmith/StateSmith/issues/362





#### PlantUML Arrows
It is generally recommended to use `-->` or `->` most often and let PlantUML figure out the best way to draw the arrows and then only use the more specific arrows when necessary.

Here's a full listing of what StateSmith supports:

| Arrow Text           | Effect                  |
| -------------------- | ----------------------- |
| `->`                 | horizontal (left/right) |
| `-->`                | vertical (up/down)      |
| `-up->` or `-u->`    | pointing up             |
| `-down->` or `-d->`  | pointing down           |
| `-left->` or `-l->`  | pointing left           |
| `-right->` or `-r->` | pointing right          |
| `--->` ... `----->`  | longer vertical lines   |





#### Extra `<<choice>>` states
You can often use extra `<<choice>>` PlantUML states to help hint at how you want things laid out like below.

> 🤓 NOTE: StateSmith is smart enough to optimize away the extra helper `<<choice>>` states. They collapse into one.

![image](https://github.com/user-attachments/assets/c56589ab-0179-487a-9d8a-ef7724ab26ef)

Combine these with arrow direction hints.
```plantuml
' choice routes
ROUTE -right-> ROUTE_BOOT1
ROUTE_BOOT1 -down-> BOOT1: [IS_STATUS(BOOT1)]
ROUTE_BOOT1 -right-> ROUTE_BOOT2: else
ROUTE_BOOT2 -down-> BOOT2: [IS_STATUS(BOOT2)]
ROUTE_BOOT2 -right-> ROUTE_RUNNING_OK: else
ROUTE_RUNNING_OK -down-> RUNNING_OK: [IS_STATUS(RUNNING_OK)]
ROUTE_RUNNING_OK -right-> ROUTE_WARNING: else
ROUTE_WARNING -down-> WARNING: [IS_STATUS(WARNING)]
ROUTE_WARNING -right-> ROUTE_ERROR: else
ROUTE_ERROR -down-> ERROR_SEQ1: / count = 3
```

## Optional Styles
```plantuml
'############################ styles ############################
' Define some colors for the states. Totally optional.
skinparam state {
    ' green style:
    BackgroundColor<<green>> 60a917
    FontColor<<green>> white

    ' yellow style:
    BackgroundColor<<yellow>> fff200
    FontColor<<yellow>> black

    ' red style:
    BackgroundColor<<red>> a20025
    FontColor<<red>> white

    ' blue style:
    BackgroundColor<<blue>> 1ba1e2
    FontColor<<blue>> white

    ' gold style:
    BackgroundColor<<gold>> f0a30a

    ' dark style:
    BackgroundColor<<dark>> 545454
    FontColor<<dark>> white
}
```


## Advanced Tips
- You can split complex state machines into multiple smaller PlantUML files using PlantUML preprocessing. See [issue 213](https://github.com/StateSmith/StateSmith/issues/213).



## Need more PlantUML syntax?
See [Contributing: PlantUML](https://github.com/StateSmith/StateSmith/wiki/Contributing:-PlantUML).
