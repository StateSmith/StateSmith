# Why StateSmith?
I couldn't find a state machine code generator that was suitable for low level embedded application use, had an attractive license, and was enjoyable to use.

Many options were painfully slow to use. You couldn't just start designing/writing code. You had to spend a bunch of time creating object oriented mappings of every little thing in an awkward outdated GUI.

One of the reasons why I like StateSmith so much is that you can just get started using it and write code how you want. You can even start with pseudo code and gradually transform it into real code using [Expansions](https://github.com/StateSmith/tutorial-2/tree/main/lesson-3). Even though StateSmith uses a GUI for drawing state machines, all of a state's functionality is written in text - no annoying awkward GUIs that slow you down.



<br>

# But Why Is It Written In C# ????
I hear you. We all have our favorite languages. I'm actually an embedded C programmer, so why did I go with C#?

**The Open Source Roslyn Compiler.** I never paid too much attention to C# until they released and documented the official C# compiler. This was huge for me! In a few lines of C# code, I can compile user code, traverse the AST, do semantic analysis, emit executable code! We actually use a subset of C# as our [Generic Intermediate Language](https://github.com/StateSmith/StateSmith/wiki/GIL:-Generic-Intermediate-Language) to make supporting multiple languages relatively easy.

That and I'm far too lazy to write StateSmith in pure C.




<br>

# Project Goals
## UML Inspired
StateSmith is based on UML2 state machines, but doesn't strive to be fully UML2 compliant. We take the core UML design and then add some improvements to it. See [UML Differences](https://github.com/StateSmith/StateSmith/wiki/UML-Differences).



<br>

## StateSmith as a Platform
Instead of 60 different state machine projects generating code for 10 different languages and slight variations, why not combine some of our efforts? If we work together, we could make something truly amazing that is fully free and open source! If you are crazy about state machines, I'd love to work together!



<br>

## Doesn't do what you want out of the box?
StateSmith is written so that you can easily swap out almost any part of it to accomplish your goals.

* ✅ Support a new input file type.
* ✅ Output a different algorithm.
* ✅ Transpile to a new language.
* ✅ Totally change the code generation step for very unique use case.
* ✅ Add custom behaviors and features...

Open an GitHub issue, GitHub discussion or head to discord if you have any questions or ideas.

