Follow build instructions first.

This page will teach you to modify `ss.cli` project and run it.

🚩 cd to `src/StateSmith.Cli/` for all of these instructions

# Run `ss.cli` and check its version
Inside of `src/StateSmith.Cli/`, run the command `dotnet run --framework=net9.0 --version`

You should see `ss.cli` run. It may prompt you about checking for updates first, but you will eventually see this:

```shell
StateSmith/src/StateSmith.Cli$ dotnet run --framework=net9.0 --version
<warnings...>
Using settings directory: /home/afk/.config/StateSmith.Cli
StateSmith.Cli 0.0.0-local-build+0ae844cb6e61adcccfd108a40ff5513f25e81d13

StateSmith/src/StateSmith.Cli$
```

Why is the `--framework` arg needed? Because `ss.cli` targets multiple dotnet runtimes. You need to pick one that you have installed.

What does `--version` do? This arg is actually given to `ss.cli`. It just prints the `ss.cli` version.


# Run `ss.cli` project creation
Run the command `dotnet run --framework=net9.0 create` (this is just like `ss.cli create`)

You should see the StateSmith project creation menu:
```
── Workflow ───────────────────────────────────────────────────────────────────
Please select your ideal workflow. We recommend starting with "User Friendly".
You can manually add a .csx file later if you need more advanced features.

> User Friendly - generally recommended. Faster & simpler. (remembered)
  User Friendly - generally recommended. Faster & simpler.
  Advanced - adds a .csx file to support advanced features.
```


# Modify `ss.cli` and Run It
Modify `src/StateSmith.Cli/Program.cs` to add a custom print message at the top of the `Main` method.

```c#
public static void Main(string[] args)
{
    Console.WriteLine("!!! MY CUSTOM STATESMITH CLI !!!");
```

Now run it again with `dotnet run --framework=net9.0`

You should see your message printed.

Note! You didn't need to build first. `dotnet run` will do that if needed.

```
StateSmith/src/StateSmith.Cli$ dotnet run --framework=net9.0
<warnings...>
!!! MY CUSTOM STATESMITH CLI !!!
Using settings directory: /home/afk/.config/StateSmith.Cli
StateSmith.Cli 0.0.0-local-build+dfdee5b9228e1107f3d8aebd0392884731d68bfd

Usage:
  <snip...>
```

