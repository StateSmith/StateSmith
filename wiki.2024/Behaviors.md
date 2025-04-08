Any state machine state or vertex can have behaviors.

A behavior could be a transition from one state to another or it could be an action to take without transitioning between states.

The below image shows 3 different types of behaviors:
* the `ON1` state will transition to `ON2` when the `INCREASE` event is dispatched
* the `ON1` state will run the code `light_on1()` when the state is entered.
* the `ON1` state will also increment variable `count` when the `ev1` event is dispatched.

![image](https://user-images.githubusercontent.com/274012/220501001-aa0c32cd-9482-46ad-8d9b-e168b16b3de2.png)

# General Behavior Syntax
Each state behavior can have:
- an optional **trigger**: can be `enter`, `exit`, an event like `do` or a user specified event. User events can be any identifier that is a valid variable name. Regex: `[a-zA-z_]\w*`. [Version 0.7.15](https://github.com/StateSmith/StateSmith/blob/c305b23fc3851e82e033d8b091de03589c96fac2/CHANGELOG.md?plain=1#L13-L14) also added support for `entry` as an alternative to `enter`. NOTE! If no trigger is specified, the `do` event will be assumed.
- an optional **guard** condition: any code that will evaluate to a boolean. If it evaluates to false, the behavior is not taken. Guard code should not have any side effects.
- an optional **action**: code that will be executed if the behavior is taken.
- an optional **transition** : transitions are drawn in the diagram.

The syntax is based on the UML 2.0 state machine syntax with a few improvements.

![image](https://user-images.githubusercontent.com/274012/220519772-bc2ca3b2-99da-4534-adfd-9e3917c35414.png)

Some examples:

![image](https://user-images.githubusercontent.com/274012/220519806-59c371a6-6b29-448e-977d-9f11241c05b3.png)

# Advanced Behavior Syntax
Here's the full syntax:

![image](https://user-images.githubusercontent.com/274012/220524407-6ea2046e-5298-4ced-9005-78a8c4695df7.png)

## [Ordering a state's behaviors.](https://github.com/StateSmith/StateSmith/blob/main/docs/diagram-features.md#extension-to-uml-support-behavior-ordering)

If a behavior isn't explicitly ordered, it will have a really high order number (1e6). Lower orders are executed first.

In the below image, `ON3` will first check behavior `1. INCREASE / count++;` and then the transition behavior transition (unordered goes last) to `BOOM`.

![image](https://user-images.githubusercontent.com/274012/220522419-c0c17dd5-956b-48da-9ab6-a9d75fe3133a.png)

## Behavior entry and/or exit points
A behavior can may specify a target [entry and/or exit point.](https://github.com/StateSmith/StateSmith/issues/3)




