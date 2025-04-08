# Goals
StateSmith is based on UML2 state machines, but doesn't strive to be fully UML2 compliant. We take the core UML design and then add some improvements to it.


# Terminology
I'm trying to balance the needs of UML users and regular users. When I write documentation, I try to avoid confusing UML terminology like "internal transitions". Most users that don't come from a UML background find the UML "internal transitions" terminology confusing because there is no actual transition between states. It also gets worse when we add in UML "local transitions" as it sounds very much like an "internal transition".

StateSmith instead uses "non-transition behaviors" to mean UML "internal transitions".

# Differences
* [local transitions only for now](https://github.com/StateSmith/StateSmith/blob/main/docs/diagram-features.md#uml-difference---local-transitions-only-for-now)
* [support behavior ordering](https://github.com/StateSmith/StateSmith/blob/main/docs/diagram-features.md#extension-to-uml-support-behavior-ordering)
* [completion events and `do` activity](https://github.com/StateSmith/StateSmith/discussions/194)
* [internal transitions](https://github.com/StateSmith/StateSmith/discussions/158)
