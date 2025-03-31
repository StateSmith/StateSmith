---
title: C#
parent: Languages
layout: default
---

# Generating C# State Machines

{: .warning }
> TODO depends on default callbacks.
> I decided to use callbacks instead of inheritance because inheritance restricts
> the user to just the default constructor, whereas callbacks allow the user
> to construct the object however they need


## Prerequisites

This tutorial assumes you have completed the following sections before proceeding:
* [Quickstart](/StateSmith/quickstart/)



## Adding actions to your state machine

We will want to be able to execute some code whenever the state machine changes state, so let's add some actions to the `Off` and `On` states.

Update your lightbulb state machine to look like the following:

```plantuml
{% include_relative Lightbulb.puml %}
```

You can see what the new states look like in the simulator.

<iframe height="300" width="600" src="gen/Lightbulb.sim.html"></iframe>


It looks like the state machine is doing what we want. Let's go write some code to use our new state machine.

## Generate the code in C#

Let's generate C# code from `Lightbulb.puml` using StateSmith:

```
% statesmith --lang=csharp Lightbulb.puml
```

## View the State Machine

Take a look at the generated files on the disk. They should look pretty similar to the ones in the links below.

* [Lightbulb.cs](gen/Lightbulb.cs): This is the generated header for your state machine. You will use this state machine in your apps.
* [Lightbulb.sim.html](gen/Lightbulb.sim.html): A simple simulator that runs your statemachine and allows you to interact with it. It's not needed, but can be handy. You can disable generation of the simulator with the `--no-sim-gen` option.


## Using the State Machine in your own app

To write an app that will use your new state machine, you will:

1. Implement a callback class that defines the `EnterOn()` and `EnterOff()` methods you referenced in `Lightbulb.puml`.
2. Instantiate the state machine with an instance of the callback.
3. Start an event loop that tickles the state machine with every tick of the loop.

But first, use `dotnet new console` to create a new c# console project.

```
% dotnet new console
```

Then create the `LightbulbCallback.cs` file:


```c#
// LightbulbCallback.cs
{% include_relative LightbulbCallback.cs %}
```

And edit the `Program.cs` file that was generated for your by `dotnet new console`.

```c#
// Program.cs
{% include_relative Program.cs %}
```

Now compile your app with the dotnet C# compiler and run it.


```
% dotnet build
% dotnet run
Press <enter> to toggle the light switch.
Press ^C to quit.
Lightbulb is off
<enter>
Lightbulb is on
<enter>
Lightbulb is off
```

Congratulations! You've now written your first app using a StateSmith state machine!
