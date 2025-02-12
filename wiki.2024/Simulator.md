The simulator is a tool that allows you to interact with the state machine and see the state machine's behavior. Very useful for understanding a state machine's behavior.

## Contributors
Special thanks to [@emmby](https://github.com/emmby) for bringing the simulator to life!

## Quirks & Limitations
The simulator is very new and will improve over time. Here's a current listing of things to be aware of:

* No user action code is run in the simulator. It simply notes the user action code that would have run.
* The simulator requires users to evaluate guard conditions manually.
* The simulator uses a 3rd party library (mermaid.js) to render the state machine. This library has limitations and doesn't render all state machine features correctly. However, the actual state machine will function correctly. See mermaid related bugs [here](https://github.com/StateSmith/StateSmith/issues?q=is%3Aissue+is%3Aopen+mermaid+label%3Abug).
* PRs welcome!
