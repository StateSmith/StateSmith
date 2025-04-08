When an event is dispatched to a state machine, the lowest active state first gets a chance to handle it. If it doesn't handle the event, then the next interested ancestor gets a chance... all the way up to the state machine root.

An event like `EV1` is handled/consumed if a state has a behavior with a trigger for `EV1` and a guard that evaluates to `true`. If no guard is specified in the diagram, the guard condition always evaluates to `true`.

See the `Consuming Events` [example online](https://statesmith.github.io/fundamentals-1/).

## Efficiency & Nice Debugging
In the below state machine, assume we are in state `C`. When event `EV1` is dispatched to the state machine, the following will happen:
1. StateSmith knows that state `C` is interested in event `EV1` so it dispatches the event to state `C` first.
2. State `C` checks its behaviors for any that have satisfied guard conditions. If the `x > 5` guard evaluates to true, it will execute `print("C")` and the event dispatch is complete.
3. If state `C`'s guard doesn't evaluate to true, then StateSmith will send the event to the next interested ancestor of state `C`. In this example, StateSmith will skip right over state `B`, and send the event to state `A`.

Skipping states that aren't interested in an event is very useful for efficiency and also debugging purposes. My first state machine code generation attempt prior to StateSmith sent the event to every single active ancestral state and it was such a pain in the butt to follow along while debugging.

![ancestor-handling drawio](https://github.com/StateSmith/StateSmith/assets/274012/9cf433dc-f408-46fa-b018-df2b63d2670f)

Now lets consider one more example. In the above state machine, assume we are in state `C` still. When event `EV2` is dispatched to the state machine, the following will happen:
1. StateSmith knows that the lowest active state `C` is NOT interested in event `EV2` so it won't bother wasting time dispatching the event to state `C`. 
2. Instead, it knows that `B` is interested in `EV2` so it dispatches the event to state `B` first.
3. If state `B` doesn't consume event `EV2`, then state `A` is skipped over and the state machine root gets a chance to handle `EV2`.


# The `do` Event Is Special
There is one exception to the above - the `do` event. The `do` event is special in that state behaviors don't normally consume it. This allows child and ancestral states to `do` some work similar to UML behavior, but without a [bunch of issues](https://github.com/StateSmith/StateSmith/discussions/194). If we are in state `C` for the below diagram, dispatching the `do` event will cause the following code to be run:
1. `calc_c_stuff();`
2. `calc_b_stuff();`
3. `calc_a_stuff();`

![do-example drawio](https://user-images.githubusercontent.com/274012/236533139-33d16c77-06c0-4525-968e-11d8df0b2d99.svg)

# Transitions
If a transition occurs, no other behaviors will be checked for any state (doesn't matter if it is the `do` event).