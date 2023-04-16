# StateSmith
StateSmith is a cross platform, free/open source tool for generating state machines in multiple programming languages. The generated code is human readable, has zero dependencies and is suitable for use with tiny bare metal microcontrollers, video games, apps, web, computers... It avoids dynamic memory allocations for the safety or performance inclined.

![pipeline-Page-2](https://user-images.githubusercontent.com/274012/228115188-7a711715-099f-4fd7-9555-a14d973add8e.png)

*The above is [my current plan](https://github.com/StateSmith/StateSmith/wiki/Multiple-Language-Support), but I'll gladly help anyone add a new language. I'm hoping contributors will help me with this effort.*



<br>

# Features and Interactive Examples 🌟
The [fundamentals-1](https://statesmith.github.io/fundamentals-1/) webpage has simple interactive examples that let you explore most StateSmith features.

[![interactive-examples-preview-small](https://user-images.githubusercontent.com/274012/230135908-ce14fd9f-c459-4b54-8c39-a3a8129956bd.gif)](https://statesmith.github.io/fundamentals-1/)



<br>

# Quick Start (all supported languages) 🚀
Want to jump right in and just try it!?
[Tutorial-2](https://github.com/StateSmith/tutorial-2) will get you up and running as quick as possible.





<br>

# Stay in the Know 📰
Our announcements [discord](https://discord.com/invite/rNxNGQXWsU) channel is updated when new tutorials and features are added.





<br>

# Please Consider Advising/Contributing 📈
If you like StateSmith and want to help improve it, your help would be very much appreciated! StateSmith is a pretty decent tool right now, but it is going to take a team to elevate it to the next level.

* You can help without writing code. StateSmith needs more [user feedback/guidance](https://github.com/StateSmith/StateSmith/wiki/User-Feedback) before it can hit version 1.0.
* If you are up for coding, check out [Contributing](https://github.com/StateSmith/StateSmith/wiki/Contributing).

Thanks!




<br>

# More Examples 🔫

Here's a demo [youtube video](https://www.youtube.com/watch?v=9czSDothuzM) showing a Laser Tag menu implemented with StateSmith and running in an Arduino simulation.

[![picture 1](images/main-lasertag-demo1.png)](https://www.youtube.com/watch?v=9czSDothuzM)

You can interact with the generated laser tag menu code using the [wokwi simulation](https://wokwi.com/projects/351165738904453719).

There are also some [old examples](./examples/README.md) in this repo. They are perfectly valid, but use an older style of running StateSmith and some use the old yEd editor (draw.io is better).

The [StateSmith-examples](https://github.com/StateSmith/StateSmith-examples) repo has a collection of various projects that explore proposals or advanced ideas.





<br>

# Is StateSmith ready for use? 🧪
StateSmith is generating working code, and has decent test (420+) and behavior specification coverage. There are 45+ specification integration tests that read a diagram file, generate executable state machine code, then compile and execute that code in another process and ensure that the state machine behavior is exactly what was expected. This strong test base gives me confidence. It also allows us to refactor and optimize StateSmith without fear of accidentally breaking specified behavior.

![picture 1](images/test-coverage-2022-12.png)  

Every aspect of state behavior is verified during specification testing.

Breaking changes and migration steps will be documented in [./CHANGELOG.md](./CHANGELOG.md) and should be pretty minimal.




<br>

# More Info 📖
There is also a slightly older quick start. Some issues covered in the video have already been fixed. It is specific to C, but has some useful info. The video is a bit lengthy, but has chapter markers to let you skip to the section you are interested in. https://youtu.be/qej8pXp3dX4

See the StateSmith [GitHub wiki](https://github.com/StateSmith/StateSmith/wiki) or ask a question.




<br>

# Need help? Suggestion? Brainstorm? 🙋
Join us on [discord](https://discord.com/invite/rNxNGQXWsU).

Feel free to open a [github issue](https://github.com/StateSmith/StateSmith/issues).

Or you can use the project's [discussion space](https://github.com/StateSmith/StateSmith/discussions).

