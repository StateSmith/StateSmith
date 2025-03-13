# Generating C++ State Machines


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

<iframe height="300" width="600" src="Lightbulb.sim.html"></iframe>


It looks like the state machine is doing what we want. Let's go write some code to use our new state machine.

## Generate the code in C++ 

Let's generate C++ code from `Lightbulb.puml` using StateSmith:

```
% statesmith --lang=c++ Lightbulb.puml
```

## View the State Machine

Take a look at the generated files on the disk. They should look pretty similar to the ones in the links below.

* [Lightbulb.hpp](Lightbulb.hpp): This is the generated header for your state machine. You will use this state machine in your apps.
* [Lightbulb.cpp](Lightbulb.cpp): This is the generated implementation for your state machine. You generally won't need to do much with the implementation, but it can be interested to inspect to see how it works.
* [Lightbulb.sim.html](Lightbulb.sim.html): A simple simulator that runs your statemachine and allows you to interact with it. It's not needed, but can be handy. You can disable generation of the simulator with the `--no-sim-gen` option.


## Using the State Machine in your own app

To write an app that will use your new state machine,
add the following contents to a new file `myapp.cpp`. This code will:

1. Implement a callback class that defines the `enter_on()` and `enter_off()` methods you referenced in `Lightbulb.puml`.
2. Instantiate the state machine with an instance of the callback.
3. Start an event loop that tickles the state machine with every tick of the loop.

```c++
// LightbulbCallback.cpp
{% include_relative LightbulbCallback.cpp %}
```

```c++
// MyApp.cpp
{% include_relative LightbulbCallback.cpp %}
```

To run your app, compile it with the C++ compiler of your choice. Here we use Gnu g++


```
% g++ -o myapp *.cpp
% ./myapp
Press <enter> to toggle the light switch.
Press ^C to quit.
Lightbulb is off
<enter>
Lightbulb is on
<enter>
Lightbulb is off
```

Congratulations! You've now written your first app using a StateSmith state machine!
