# Actions and Triggers

When an event is dispatched the state machine responds by performing actions, such as changing a variable, performing I/O, invoking a function, generating another event instance, or changing to another state.


## A Simple Example

```
{% include_relative lightbulb.puml %}
```

<iframe height="300" width="600" src="https://emmby.github.io/StateSmith/statemachine_reference/actions/lightbulb.sim.html"></iframe>


## Trigger and action syntax

The syntax for actions and triggers is:

`trigger / action`

TODO is this supported?

One or more can be nested inside a block:

```
{
    trigger / action
}
```

## Supported Triggers

StateSmith supports the following triggers:
* `entry`
* `exit`
* user-defined events (learn more in the Events tutorial)


## Executing custom code in an action

Generally speaking, you will define a callback in your app, and then call that function
as an action in your state machine diagram.

The exact details vary by language, see the docs for your language of choice for more details.


## See Also 

See https://en.wikipedia.org/wiki/UML_state_machine for a reference on state machine grammar. Not all features may be supported by StateSmith.


