# Support Matrix
StateSmith can support multiple code generation algorithm strategies so that you can choose the one that best suits your design.

The `Balanced2` algorithm is our new default algorithm (see below).

|            | `Balanced1`<br>(function pointers) | `Balanced2`<br>(switch/case) |
| ---------- | ----------- | ----------- |
| C/C++      | ✅           | ✅           |
| C++11      | (not yet)    | ✅           |
| C#         | ✅           | ✅           |
| JavaScript | ✅           | ✅           |
| TypeScript | (not yet)   | ✅           |
| Java       | (not yet)   | ✅           |
| Python     | (not yet)   | ✅           |

<br>



# Selecting An Algorithm
The current default changed to `Balanced2` in lib version `0.13.0`, but you can continue using `Balanced1` if you wish using the settings below.

## TOML
```toml
[SmRunnerSettings]
algorithmId = "Balanced1"
```
## CSX
You can use toml settings, `SmRunner.Settings` or the constructor.
```cs
SmRunner smRunner = new(diagramPath: somePath, renderConfig: someRenderConfig, transpilerId: someTranspilerId, algorithmId: AlgorithmId.Balanced1);
smRunner.Settings.algorithmId = AlgorithmId.Balanced1;
```

<br>



# ✅ `Balanced2`

The Balanced2 algorithm is a variant of Balanced1. Instead of dynamically updating function pointers, Balanced2 uses a more traditional `switch/case` mapping.

Pros:
* minimal RAM usage (as low as 1 byte per state machine instance)
* looks more like hand written state machine code (easier to follow)

Cons:
* may use more stack for large deeply nested designs although Tail Chain Optimization (TCO) may eliminate this.
* a bit slower than function pointers
* might use a bit more code space

---


<br>

# ✅ `Balanced1`
The current Balanced1 algorithm tries to balance the following:
* performance - function/method pointers are quick. Also, events are only dispatched to states that are interested in them. This also helps with debugging.
* easy to understand - the generated code explains what is happening and why.
* good debugging - some older algorithms made it very annoying to debug Hierarchical State Machines (HSMs). A single event would be dispatched to each and every state in the active hierarchy even if none of them cared about the event. The Balanced1 approach only dispatches events to states that care about them.
* can handle large designs - it can handle hundreds of states with deep nesting efficiently.
* low RAM usage - it could be lower, but it isn't bad. More on this below.
* low code size

## RAM usage
Each state machine instance has a few pointers and an array of pointers the size of the event count. In my personal experience, state machines that have lots of instances generally don't have many events so the cost is small, but that might not be the case for everything. One example is the button debouncing state machine that I use 4 of for the laser tag demo. It only listens to a single event so the multiplied cost by event count is negligible.

[This page has some alternative Ideas](https://github.com/StateSmith/StateSmith/wiki/Z:-algorithm-misc).

---

# [AlgoFlattened1 - TODO](https://github.com/StateSmith/StateSmith/issues/116)
The idea here is that we transform the state machine graph to flatten the nesting of states (hierarchy).

This would be great for small designs, but awful for large deeply nested designs.

Pros:
* faster execution
* a bit simpler to follow
* useful for FPGA targets

Cons:
* duplicates code (terrible for large diagrams)
* can't set a breakpoint in parent behavior as it is duplicated for all its descendants.

