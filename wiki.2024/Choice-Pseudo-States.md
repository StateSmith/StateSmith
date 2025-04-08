https://github.com/StateSmith/StateSmith/issues/40

StateSmith now supports Choice Pseudo States / Choice Points with `else`.

Choice points are not states. They can only be passed through during a transition.
This requires that they always have a default transition which is usually specified as `else`, but
it could also just be a default true transition with a higher order. In fact, `else` is just converted to a very large order number (see `Behavior.ELSE_ORDER`).

![image](https://user-images.githubusercontent.com/274012/218221391-ef332a88-cc4a-4b1c-ac19-98a062cbad49.png)

A choice diagram node can have any shape as long its text matches either `$choice` or `$choice : some_label`. StateSmith also supports [PlantUML choice states](https://plantuml.com/state-diagram#8bd6f7be727fb20e).

Choice point labels are optional and do not have to be unique.

You can choose to hide a `$choice` label.



## Initial States, Entry Points and Exit Points now behave like Choice Points
StateSmith 0.5.9 had a limitation of a single transition for initial states, entry points, exit points, but this has been lifted. These pseudo states now behave a lot like Choice Points.

![image](https://user-images.githubusercontent.com/274012/218221874-4f358815-d819-4f40-8352-94f6de15c11a.png)

```
Dispatch event EV5
===================================================
State PARENT: check behavior `EV5 / { count++; }`. Behavior running.

Dispatch event EV1
===================================================
State S1: check behavior `EV1 TransitionTo(G)`. Behavior running.
Exit S1.
Transition action `` for S1 to G.
Enter G.
Transition action `` for G.InitialState to G_S1.
Enter G_S1.

Dispatch event EV2
===================================================
State G: check behavior `EV2 TransitionTo(PARENT.InitialState)`. Behavior running.
Exit G_S1.
Exit G.
Transition action `` for G to PARENT.InitialState.
Transition action `` for PARENT.InitialState to S1.
Enter S1.
```
*Above from Spec2Sm specification tests.*

![image](https://user-images.githubusercontent.com/274012/218221898-0cde705e-af3d-4822-8a33-a4b078ca5565.png)

![image](https://user-images.githubusercontent.com/274012/218221926-5508bd8e-1935-43fc-9142-65679e3b9626.png)
