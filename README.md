# StateSmith
StateSmith is a cross platform, free/open source tool for generating state machines in multiple programming languages. The generated code is human readable, has zero dependencies and is suitable for use with tiny bare metal microcontrollers, video games, apps, web, computers... It avoids dynamic memory allocations for the safety or performance inclined.

![pipeline-Page-2 (1)](https://github.com/user-attachments/assets/962090ae-b602-4915-8c4b-d28e15556ee8)

*The above is [my current plan](https://github.com/StateSmith/StateSmith/wiki/Multiple-Language-Support), but I'll gladly help anyone add a new language. I'm hoping contributors will help me with this effort. It is tricky though...*



<br>

# Features and Interactive Examples 🌟
The [fundamentals-1](https://statesmith.github.io/fundamentals-1/) webpage has simple interactive examples that let you explore most StateSmith features.

[![interactive-examples-preview-small](https://user-images.githubusercontent.com/274012/230135908-ce14fd9f-c459-4b54-8c39-a3a8129956bd.gif)](https://statesmith.github.io/fundamentals-1/)



<br>

# Quick Start (all supported languages) 🚀
Want to jump right in and just try it!?

The below tutorials use new StateSmith features that are more user friendly. They use different diagram tools, but mirror each other fairly closely otherwise.

- [Tutorial-3](https://github.com/StateSmith/tutorial-3) - `PlantUML` `CLI` `text lessons`
- [Tutorial-4](https://github.com/StateSmith/tutorial-4) - `draw.io` `CLI` `text lessons & 🎥 video series`
- [older...](https://github.com/StateSmith/StateSmith/wiki/Learning-Resources)



<br>

# Why State Machines and StateSmith? 🤔
If you are new to state machines, then prepare to level up your toolbox! They are incredibly helpful for [certain applications](https://github.com/StateSmith/StateSmith/wiki/App-Notes).

Why StateSmith?
I couldn't find a quality state machine code generator that met my needs, had an attractive license, and was enjoyable to use.

## The Diagram is Always Accurate! 📚
Before I created StateSmith, it was always a pain trying to manually synchronize a hand written state machine with a drawing. Urgent client requests come in and you update the code, but do you and your team always remember to update the drawing? Probably not and so the rot begins. Documentation trust issues arise and as designs get larger, the effort to ensure the diagram is accurate starts to become quite punishing.

Now that we use StateSmith at my work, I never have to worry about the above. I love generating fully working code **from** the documentation. Incredibly helpful for teams and communicating with clients.





<br>

# More Examples 🔫
The [StateSmith-examples](https://github.com/StateSmith/StateSmith-examples) repo has a growing list of examples showcasing different [application uses](https://github.com/StateSmith/StateSmith/wiki/App-Notes).

[![mario-sm](https://user-images.githubusercontent.com/274012/234160417-c2fcb028-0c7f-465f-b453-b04a53b48bcf.gif)](https://github.com/StateSmith/StateSmith-examples)



<br>

# Is StateSmith ready for use? 🧪
> We use StateSmith in a fair number of production projects at my work. It's been super helpful.<br>
> Other companies are using StateSmith in production as well (consumer electroncics, autonomous vehicles, ...).

StateSmith has a strong suite of tests (730+) and behavior specification coverage. The specification integration tests read a diagram file, generate executable state machine code, then compile and execute that code in another process and ensure that the state machine behavior is exactly what was expected. The same suite of integration tests run for each supported programming language. This strong test base gives me confidence. It also allows us to refactor and optimize StateSmith without fear of accidentally breaking specified behavior.




<br>

# More Info 📖
The StateSmith [GitHub wiki](https://github.com/StateSmith/StateSmith/wiki) has a good amount of documentation right now, but always feel free to ask a question.

YouTube channel: https://www.youtube.com/@statesmith


<br>

# Need help? Suggestion? Brainstorm? 🙋
Join us on [discord](https://discord.com/invite/rNxNGQXWsU).

Feel free to open a [github issue](https://github.com/StateSmith/StateSmith/issues).

Or you can use the project's [discussion space](https://github.com/StateSmith/StateSmith/discussions).

