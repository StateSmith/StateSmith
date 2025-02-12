StateSmith is a cross platform, free/open source tool for synchronizing state machine diagrams to multiple programming languages. The generated code is human readable, has zero dependencies and is suitable for use with tiny bare metal microcontrollers, video games, apps, web, computers... It avoids dynamic memory allocations for the safety or performance inclined.

With StateSmith, your state machine diagram is code. Never worry about keeping your statemachine documentation in sync with your code again.

[TODO convert this to mermaid]
![misc documentation-main page](https://github.com/user-attachments/assets/42fdb74b-0cd1-43b4-8103-7fd412ca7397)



# Features

[A quick summary of features, a few svgs, and links to interactive examples]


# Getting Started

It's easy to get started with StateSmith.

1. First, write or draw a statemachine in the tool of your choice
```
# hello.mmd
a -> b
```
 2. run `statesmith hello.mmd`
 3. add the resulting state machine code to your C, C++, C#, Java, JavaScript, or TypeScript app.

Learn more in our [Quickstart](https://emmby.github.io/StateSmith/) tutorial.



# Why Use StateSmith?

I couldn't find a quality state machine code generator that met my needs, had an attractive license, and was enjoyable to use, so I wrote StateSmith.

If you are new to state machines, then prepare to level up your toolbox! 

### Powerful and Production ready

StateSmith has a strong suite of tests (730+) and behavior specification coverage. The specification integration tests read a diagram file, generate executable state machine code, then compile and execute that code in another process and ensure that the state machine behavior is exactly what was expected. The same suite of integration tests run for each supported programming language. StateSmith has been starred the most of any state machine library on Github, and is in production use by several companies.


### Easy to use
statesmith is incredibly easy to use.

 ...

You can easily try out your statemachine using the built in simulator.

### Works with the tools you already use
statesmith doesn't try to take over your entire workflow. It does not replace your IDE, require you to write tests in a custom language, or have its own build tool. It works with the tools you're already using, like GitHub, VS Code, Visual Studio, JUnit, Jest, etc.

See [Integrations]() for recipes to integrate statesmith into your tooling.


### The Diagram is always accurate!

Before I created StateSmith, it was always a pain trying to manually synchronize a hand written state machine with a drawing. Urgent client requests come in and you update the code, but do you and your team always remember to update the drawing? Probably not and so the rot begins. Documentation trust issues arise and as designs get larger, the effort to ensure the diagram is accurate starts to become quite punishing.

Now that we use StateSmith at my work, I never have to worry about the above. I love generating fully working code **from** the documentation. Incredibly helpful for teams and communicating with clients.

