---
title: Python
parent: Languages
layout: default
---

# Generating Python State Machines

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
{% include_relative lightbulb.puml %}
```

You can see what the new states look like in the simulator.

<iframe height="300" width="600" src="gen/lightbulb.sim.html"></iframe>


It looks like the state machine is doing what we want. Let's go write some code to use our new state machine.

## Generate the code in Python

Let's generate Python code from `lightbulb.puml` using StateSmith:

```
% statesmith --lang=python lightbulb.puml
```

## View the State Machine

Take a look at the generated files on the disk. They should look pretty similar to the ones in the links below.

* [lightbulb.py](gen/lightbulb.py): This is the generated header for your state machine. You will use this state machine in your apps.
* [lightbulb.sim.html](gen/lightbulb.sim.html): A simple simulator that runs your statemachine and allows you to interact with it. It's not needed, but can be handy. You can disable generation of the simulator with the `--no-sim-gen` option.


## Using the State Machine in your own app

To write an app that will use your new state machine, you will:

1. Implement a callback class that defines the `enter_on()` and `enter_off()` methods you referenced in `lightbulb.puml`.
2. Instantiate the state machine with an instance of the callback.
3. Start an event loop that tickles the state machine with every tick of the loop.

Your state machine needs a callback object that contains the `enter_on()` and `enter_off()` functions you referenced in your diagram. StateSmith assumes this callback is in a file named `lightbulb_callback.py` (this can be changed via [settings](/advanced/settings.html)), so create that file with the following contents:

```python
// lightbulb_callback.py
{% include_relative lightbulb_callback.py %}
```

And create a `myapp.py` file that will contain your main method and start your state machine.

```python
// myapp.py
{% include_relative myapp.py %}
```

Now run your app with python.


```
% python3 myapp.py
Press <enter> to toggle the light switch.
Press ^C to quit.
Lightbulb is off
<enter>
Lightbulb is on
<enter>
Lightbulb is off
```

Congratulations! You've now written your first app using a StateSmith state machine!
