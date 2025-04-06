NOTE: this feature is less important now that we have automatic name conflict resolution enabled by default.
https://github.com/StateSmith/StateSmith/wiki/State-Names

You may still want to use $mod prefix to have very concise and very short prefixes however.

https://github.com/StateSmith/StateSmith/issues/65

Example project: https://github.com/StateSmith/StateSmith-examples/tree/main/modding-prefix

This feature is **experimental** right now. Looking for user feedback.

TLDR: StateSmith will prepend state names with prefixes if the following text is found in a state's behavior:
* `prefix.add(<arg>)` - appends the argument to the prefix used for sub states. Useful when you want to explicitly specify the prefix to add.
* `prefix.auto()` - appends the current state's name to the prefix used for sub states. Useful when the parent state name is already fairly short.
* `prefix.set(<arg>)` - clears any previous prefix and sets it to the argument.

### Background and Explanation
Currently all states must be named uniquely within a state machine.

This is fine for smaller designs, but becomes more of a burden with large deeply nested designs. You can get around it by prefixing states with their parent names which can get a bit tedious.

In the below design, the state name `NONE` is used twice. This will result in a validation error.

![image](https://user-images.githubusercontent.com/274012/218154518-97ef5a0c-e99e-49ed-93bd-2779b133c0cf.png)


```
VertexValidationException: Duplicate state name `NONE` also used by state `Statemachine{SomeSmName}.State{ORDER_MENU}.State{BEVERAGE}.State{NONE}`.
    Vertex
    Path: Statemachine{SomeSmName}.State{ORDER_MENU}.State{VEG}.State{NONE}
    Diagram Id: n0::n4::n2::n0
    Children count: 0
    Behaviors count: 1
    Incoming transitions count: 1
```

One solution is to manually prefix all states as shown below. Nothing technically wrong with this, but it quickly becomes a burden as nesting increases. There is a better solution using the `prefix` methods.

![image](https://user-images.githubusercontent.com/274012/218154568-9d625898-b26a-48fb-81e2-050a13f58bbe.png)

The below diagram uses special `$mod` `prefix` methods to tell StateSmith how to prefix sub states.

![image](https://user-images.githubusercontent.com/274012/218154610-82bb04a4-4771-47b3-b6ad-6c9a3c2fef15.png)

The generated state names will be as follows:

* `ORDER_MENU`
* `OM__BEVERAGE`
* `OM__BEV__NONE`
* `OM__BEV__TEA`
* `OM__BEV__WATER`
* `OM__VEG`
* `OM__VEG__NONE`
* `OM__VEG__POTATO`
* `OM__VEG__YAM`