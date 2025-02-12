# Generating C State Machines

## Prerequisites

This tutorial assumes you have completed the following sections before proceeding:
* [Quickstart](docs/quickstart/)



## Adding actions to your state machine

We will want to be able to execute some code whenever the state machine changes state, so let's add some actions to the `Off` and `On` states.

Update your lightbulb state machine to look like the following:

```
{% include_relative lightbulb.puml %}
```

You can see what the new states look like in the simulator.

<iframe height="300" width="600" src="https://emmby.github.io/StateSmith/languages/c/lightbulb.sim.html"></iframe>


It looks like the state machine is doing what we want. Let's go write some code to use our new state machine.

## Generate the code in C 

Let's generate C code from `lightbulb.puml` using StateSmith:

```
% statesmith --lang=c99 lightbulb.puml
```

## View the State Machine

Take a look at the generated files on the disk. They should look pretty similar to the ones in the links below.

* [lightbulb.h](lightbulb.h): This is the generated header for your state machine. You will use this state machine in your apps.
* [lightbulb.c](lightbulb.c): This is the generated implementation for your state machine. You generally won't need to do much with the implementation, but it can be interested to inspect to see how it works.
* [lightbulb.sim.html](lightbulb.sim.html): A simple simulator that runs your statemachine and allows you to interact with it. It's not needed, but can be handy. You can disable generation of the simulator with the `--no-sim-gen` option.

Open the generated `lightbulb.sim.html` simulator in your web browser to verify your state machine works the way you expect. 


## Using the State Machine in your own app

To write an app that will use your new state machine,
add the following contents to a new file `myapp.c`. This code will:
1. Instantiate the state machine struct.
2. Start an event loop that tickles the state machine with every tick of the loop.
3. Implement callbacks to do things as the machine enters different states.

```c
/* myapp.c */
{% include_relative myapp.c %}
```

To run your app, compile it with the C compiler of your choice. Here we use Gnu CC


```
% gcc -o myapp -Wno-error=implicit-function-declaration *.c
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


## Getting rid of that pesky implicit-function-declaration error.

The previous code works, but it causes some errors in the compiler. We had to disable the `implicit-function-declaration` warning in `gcc` to get it to compile.

Let's fix this with the `.inc.h` technique that is commonly used in situations like this but may be less familiar to many C developers, which is why we didn't start with it.

Generate the state machine as before, but rename the `.c` file to `.inc.h`:

```
% statesmith --lang=c99 lightbulb.puml
```
```
% mv lightbulb.c lightbulb.inc.h
```

Create a new `myapp2.c` file that contains the following code. It is identical
except for the part labeled `NEW NEW NEW`.

```c
/* myapp2.c */
{% include_relative myapp2.c %}
```

Now compile the app. Voila! No errors.
```
% gcc -o myapp2 myapp2.c
% ./myapp2
```


> [!INFO]
> Why does this work?
> 
> In the first example, `myapp.c` and `lightbulb.c` were both `.c` files and expected to
> be fully specified and independently compilable. This was not the case for `lightbulb.c`
> because it was missing type declarations for `enter_on()` and `enter_off()`.
>
> We can overcome this problem by including `lightbulb.c` in `myapp2.c`. Together, 
> `myapp2.c` has all the declarations needed to be a completely-specified C file.
> However, it's unexpected to include one C file from another, so a convention
> has emerged to name the included file `.inc` or `.inc.h`.
>
> Either extension is fine. We recommend using `.inc.h` for some tooling like Arduino IDE
> which doesn't know about `.inc` files.
