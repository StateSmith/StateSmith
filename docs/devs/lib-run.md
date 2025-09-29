The core project `src/StateSmith` is a library C# project. It can't be run on its own.

You have a few choices for running it:
* run it with the one of the C# test projects (`src/StateSmithTest` or `src/StateSmith.CliTest`). Write a unit test that uses the lib.
* run it with the `ss.cli` project `src/StateSmith.Cli`. Use your custom `ss.cli` to run the lib.
* run it with an external .csx file. See [lib-with-csx.md](./lib-with-csx.md).
* run it with your own C# project...
