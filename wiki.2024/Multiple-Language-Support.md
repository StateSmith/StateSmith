Supported Languages:
1. ✅ C99
1. ✅ C11++ (class)
1. ✅ C++ (C style)
1. ✅ C#
1. ✅ JavaScript
1. ✅ TypeScript
1. ✅ Java
1. ✅ Python 3.10+

Other's to support:
1. go
1. rust
1. php
1. verilog (community contribution)
1. Lua
1. C89 (for old Linux kernel code)
...

# GIL
To help support multiple different target languages, we are using a Generic Intermediate Language GIL.

The selected state machine algorithm outputs GIL code, and then a transpiler converts GIL to the desired output language.

Right now, our GIL code is just a subset of C# so that we can use the excellent Roslyn tools. I also don't want to create another language, parser, AST...

![image](https://github.com/user-attachments/assets/4df919fa-96e3-4212-b2f6-4f815fcdfd79)


# Two Steps
The current approach is to split the code generation step into 2 steps:
1. IGilAlgo- A state machine coding algorithm. Generates state machine code in a Generic Intermediate Language (GIL). Different algorithms will be developed to prioritize different things (balanced, speed, ram usage, code size...). It may choose to use function pointers, or switch statements...
1. IGilTranspiler - Different classes will translate GIL to different output languages (GilToC99, GilToCSharp, GilToJs, GilToJava).

We can also have specialized code generators that only work for particular languages. I'd like to limit these though as it increases the maintenance effort.


