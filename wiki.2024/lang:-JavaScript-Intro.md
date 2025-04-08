This is a quick intro to StateSmith for JavaScript/TypeScript developers. We also support C, C++, C#, Java, and Python.

StateSmith is a free and open source program that turns your state machine diagrams into code.

![image](https://github.com/user-attachments/assets/7ddaccbc-936c-4e30-b738-7d3318a186b0)

The generated state machines have zero dependencies, are easy to read/debug, and can run anywhere (web, nodejs, embedded script...). It only requires ECMAScript 5 (ES5) which was released in 2009.

A single diagram is transformed into a single `.js` file that is ready to be used in your project.

![image](https://github.com/user-attachments/assets/314f2946-66d8-4169-a1e4-9e46160c1a2a)

<!-- ![image](https://github.com/user-attachments/assets/780bbdba-1df0-4dd1-b8d0-d8c6ae4a6128) -->

<br>

# JavaScript Applications
Reasons you might try StateSmith as a JavaScript developer:
- You use state machines in multiple programming languages. Learn one tool that can generate code for all of them.
- You want powerful diagramming tools that work offline and don't require a paid subscription.
- You think your state machine may grow to a medium or large size.
- You are interested in game development, IoT, robotics, embedded, ...

I've really enjoyed using StateSmith for modelling relatively complex enemy "AI" for a js game I'm working on. The enemy blobs have 15 high level behaviors. I'll be releasing a tutorial on this soon.

![shiny-and-dive](https://github.com/user-attachments/assets/26f01e71-9ec5-43c7-b4e0-1ca9c1f47782)

StateSmith has a lot of features that make it easy to work with both simple and complex state machines. The below `draw.io` diagram shows the high level states of my enemy blob "AI". Notice how the `IDLE` and `HUNTING` states are visually collapsed. You can jump into them to see their sub-states. This is super helpful for managing large complex state machines.

![image](https://github.com/user-attachments/assets/0393e861-ffb2-4b92-852d-79e2532404d8)

StateSmith is also handy for smaller state machines that have lots of transitions between states (e.g. a game character with many animations). One nice thing about draw.io and StateSmith is that you can embed images/gifs in your diagrams if you want.

![mario](https://github.com/user-attachments/assets/f5835d30-88da-4e5e-8085-a8a31d08cd75)



<br>


# Features and Interactive Examples 🌟
The [fundamentals-1](https://statesmith.github.io/fundamentals-1/) webpage has simple interactive examples that let you explore most StateSmith features.

[![interactive-examples-preview-small](https://user-images.githubusercontent.com/274012/230135908-ce14fd9f-c459-4b54-8c39-a3a8129956bd.gif)](https://statesmith.github.io/fundamentals-1/)


<br>


# Quick Start 🚀
Want to jump right in and just try it?

The below tutorials use different diagram tools (`PlantUML` or `draw.io`), but otherwise mirror each other closely.

- [Tutorial-3](https://github.com/StateSmith/tutorial-3) - `PlantUML`. A good choice if you like text based diagramming.
- [Tutorial-4](https://github.com/StateSmith/tutorial-4) - `draw.io`. Has text and video 🎥 lessons. Good if you want more diagram control or will be making large designs.


<br>


# Main Repo
Check out https://github.com/StateSmith/StateSmith






<br>

# Comparison to XState
First of all, I want to say that I think David Khourshid (the creator of [XState](https://github.com/statelyai/xstate)) is a really good guy. He's done a lot for the state machine js community and has a lot of passion for state machines. I hope his commercialization of XState via [Stately](https://stately.ai/) is successful. It would be nice to see more people making a living with open source software.

For this comparison, keep in mind that I'm an embedded systems developer that enjoys some web/app/game development on the side. I'm not a professional web developer.

Please let me know if I make any mistakes.

## General Tool vs Specialized Tool 🛠️
StateSmith is more of a swiss army knife for state machines. It has a lot of features and supports many programming languages, but isn't specialized for any one use case. While StateSmith does have a lot of interactive learning resources & tutorials, it does not yet have tutorials for specific web frameworks.

In contrast, XState focuses only on JavaScript/TypeScript and has tie-ins/tutorials for popular web frameworks.

There is some overlap in features between StateSmith and XState/Stately, but each have unique features.

## Free vs Paid 💰
StateSmith is fully free and open source. This includes the diagramming and simulation tools.

XState is free and open source, but you really want a paid subscription to `Stately` if your design approaches a "medium" size. Without a paid subscription, `Stately` is unusable for many users as your diagrams must be public. 

A paid subscription isn't necessarily a bad thing though (if you can afford it). One of the big benefits of `Stately` being a paid service is that they have staff working on it full time. This means they can provide better support and add more features. In contrast, StateSmith is largely just Adam (me) working on it in my free time :) StateSmith has had some great contributions from the community, but we don't yet have a core team of open source developers.

## Managing Complexity ⚖️
If your state machine is small, you are likely to be more productive in XState (or hand coding).

StateSmith really starts to shine when your state machine grows to a medium or large size.



<br>
<br>
<br>

Thanks for reading! I hope you found this interesting. If you have any questions, feel free to ask. I'm happy to help.