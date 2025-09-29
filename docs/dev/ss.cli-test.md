See [./run-ss.cli.md](./run-ss.cli.md) first.

This page will teach you to modify `ss.cli` tests and run them.

🚩 cd to `src/StateSmith.CliTest/` for all of these instructions.

NOTE! This is `StateSmith.CliTest` and not `StateSmith.Cli` directory. We need the test project.

# Run Tests
This is pretty easy. Just run `dotnet test`.

Note: this will automatically build `StateSmith.Cli` if needed.

You'll see some output like below.

```shell
src/StateSmith.CliTest$ dotnet test
<snip...>
Test summary: total: 87, failed: 0, succeeded: 87, skipped: 0, duration: 2.3s
Build succeeded with 2 warning(s) in 4.0s
```
