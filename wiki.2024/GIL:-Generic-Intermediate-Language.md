# Intro
To help support multiple different target languages, we are using a Generic Intermediate Language GIL.

The selected state machine algorithm outputs GIL code, and then a transpiler converts GIL to the desired output language.

Right now, our GIL code is just a subset of C# so that we can use the excellent Roslyn tools. I also don't want to create another language, parser, AST...


<br><br><br><br>

# You don't need to read the rest
This is an advanced topic for contributors. Regular users of StateSmith don't need to understand the nitty gritty of the stuff in this page.

See also https://github.com/StateSmith/StateSmith/wiki/Z:-mixing-GIL-and-user-code

This page is also currently a collection of thoughts and not fully updated.

<br><br><br><br>


# GIL Method Pointers or Function Pointers?
We have to decide whether GIL should output Method Pointers or Function Pointers.

If we output Method Pointers, low level transpilers (like for C) have to convert them to raw function pointers, but this isn't too hard.

```c#
// GIL
private void TEST2_S2_exit()
{
    // adjust pointers for this state's exit
    this.currentStateExitHandler = TEST2_ROOT_exit;
    this.currentEventHandlers[(int)EventId.EV1] = TEST2_ROOT_ev1;
}
```

```c
//C
static void TEST2_S2_exit(Sm* sm)
{
    // adjust pointers for this state's exit
    sm->currentStateExitHandler = TEST2_ROOT_exit;
    sm->currentEventHandlers[(int)EventId.EV1] = TEST2_ROOT_ev1;
}
```

If we output Function Pointers, higher level languages miss out on a great opportunity to avoid expansions (see below section).

Or they have to convert Function Pointers to Method Pointers, but this is a lot more work. It would probably require another compilation pass to do just this conversion. Slows things down and adds complexity.

```c#
// GIL - function pointer version
private static readonly Func EAT_BURRITO_right = (UiSm sm) =>
{
    // EAT_BURRITO behavior
    // uml: RIGHT / { burritoCount--; } TransitionTo(COOKING_FOOD)
    {
        //..

        // Step 2: Transition action: `burritoCount--;`.
        sm.burritoCount--;

        // ...
    } // end of behavior for EAT_BURRITO
};
```

```c#
// C# - more effort to convert to
private void EAT_BURRITO_right()
{
    // EAT_BURRITO behavior
    // uml: RIGHT / { burritoCount--; } TransitionTo(COOKING_FOOD)
    {
        //..

        // Step 2: Transition action: `burritoCount--;`.
        burritoCount--;

        // ...
    } // end of behavior for EAT_BURRITO
};
```

## Static Functions Require Knowing Name of `this` Parameter
Another significant benefit of using Method Pointers is that there is no uncertainty about how to access state machine variables. For example, the history vertex processor needs to
add behaviors that reference state machine variables. If we output GIL with static functions and function pointers, then the history vertex processor needs to know what the state machine parameter is called.

```c#
// GIL - History processor needs to know name `sm`
private static void T7__DH1__HERO_enter(Spec2Sm sm)
{
    //...

    // T7__DH1__HERO behavior
    // enter / { sm.vars.T7__DH1__ALIENS_DETECTED_history = T7__DH1__ALIENS_DETECTED_HistoryId.T7__DH1__HERO; }
    {
        // This code is added by history processor, but `sm` name is chosen by state machine algorithm.
        sm.vars.T7__DH1__ALIENS_DETECTED_history = T7__DH1__ALIENS_DETECTED_HistoryId.T7__DH1__HERO;
    } // end of behavior for T7__DH1__HERO
}
```

```c#
// GIL - `this` is more convenient for things like History Processor.
private void T7__DH1__HERO_enter()
{
    //...

    // T7__DH1__HERO behavior
    // enter / { this.vars.T7__DH1__ALIENS_DETECTED_history = T7__DH1__ALIENS_DETECTED_HistoryId.T7__DH1__HERO; }
    {
        // History processor can simply use `this`.
        this.vars.T7__DH1__ALIENS_DETECTED_history = T7__DH1__ALIENS_DETECTED_HistoryId.T7__DH1__HERO;
    } // end of behavior for T7__DH1__HERO
}
```

## Method Pointers are Amazing for Higher Level Languages

Function pointers are fine when we are targeting the C language, but prevent some awesome usage for higher level languages.

Emulating function pointers in C# looks like this:
```c#
// Requires expansion from `burritoCount` to `sm.burritoCount`
private static readonly Func EAT_BURRITO_right = (UiSm sm) =>
{
    // EAT_BURRITO behavior
    // uml: RIGHT / { burritoCount--; } TransitionTo(COOKING_FOOD)
    {
        //..

        // Step 2: Transition action: `burritoCount--;`.
        sm.burritoCount--;

        // ...
    } // end of behavior for EAT_BURRITO
};
```

This works OK, but requires that `burritoCount` have an expansion or we have to write `sm.burritoCount` in the diagram. Not the end of the world, but if it was a method instead, then we could simply write `burritoCount` without any expansion at all!

```c#
// NO expansion needed!
private void EAT_BURRITO_right()
{
    // EAT_BURRITO behavior
    // uml: RIGHT / { burritoCount--; } TransitionTo(COOKING_FOOD)
    {
        //..

        // Step 2: Transition action: `burritoCount--;`.
        burritoCount--;

        // ...
    } // end of behavior for EAT_BURRITO
};
```

Expansions are great when needed, but we should absolutely avoid them unless required so that we can keep the user experience productive and enjoyable.

## Decision
In general, I find it a lot easier to convert from High Level code to Low Level code so I'm going to try using Method Pointers instead of Function Pointers in GIL.

It also helps simplify the state machine algorithms that output GIL code.




---

# Method Pointer Efficiency
## C
C supports raw function pointers which are great for speed. Most other languages listed do not.

## C#
C# has delegates, but using them as a direct replacement for C function pointers results in delegates being created and then garbage collected which isn't great. We can get around this by statically creating all the necessary delegates though.

- https://stackoverflow.com/questions/2082735/performance-of-calling-delegates-vs-methods
- https://devblogs.microsoft.com/dotnet/understanding-the-cost-of-csharp-delegates/

## JavaScript
Nothing fancy required for js. Method references work great without any apparent conversions :)

```js
class MyObj
{
    constructor(name) {
        this.name = name;
    }

    m1() {
        console.log("m1 name: " + this.name);
    }
}

let a = new MyObj("a");
let b = new MyObj("b");

a.m1 == a.m1;   // true
a.m1 == b.m1;   // true

let am1 = a.m1;
let bm1 = b.m1;

am1 == bm1; // true

a.m1(); // m1 name: a
b.m1(); // m1 name: b

am1(); // Uncaught TypeError: Cannot read properties of undefined (reading 'name')
bm1(); // Uncaught TypeError: Cannot read properties of undefined (reading 'name')

```

## Java
Java doesn't have delegates and would require something like lambdas. Would probably want to statically create all of them like mentioned above for C#.
- https://stackoverflow.com/questions/39346343/java-equivalent-of-c-sharp-delegates-queues-methods-of-various-classes-to-be-ex

```java
// java
// good info: http://www.java2s.com/Tutorials/Java/Java_Lambda/0070__Java_Functional_Interface.htm
// demo: https://onlinegdb.com/YHzPHes4Vy
class Main {
    @FunctionalInterface
    interface Func {
        public int execute(int a, int b);
    }

    public static Func a = (int a, int b) -> { return a + b; };
    public static Func b = (int a, int b) -> { return a + b; };

    public static void main(String[] args) {
        System.out.println("Hello, World! " + b.execute(4, 90));
        System.out.println("a == b " + (a == b)); // false
        System.out.println("a == a " + (a == a)); // true
    }
}
```



## Index to Functions
An alternative to using function pointers/delegates is to use function indexes something like below.


```c
// C99. func_id would probably be an enum actually.
void execute_func_id(Spec1Sm* self, int func_id)
{
    switch (func_id)
    {
        default:
        case 0: break;
        case 1: S11_ev1(self); return;
        case 2: T11_ev2(self); return;
    }
}
```

### Pros
* This has the benefit of also saving RAM for embedded C targets. Enum size (probably 1 byte) vs function pointer size (usually 4 bytes) multiplied by element count of `Spec1Sm_Func current_event_handlers[Spec1Sm_EventIdCount];`.
* This will work for all languages except Python (until 3.10) and Lua.
* Safer (no function pointer that can be corrupted).

### Cons
* Slower
* Less enjoyable to debug (jumps around a bit more).


## Switch Statements
Most languages support this. Python does starting at 3.10, but can use dictionaries before that. Lua doesn't and would need to use a mapping.

-----

