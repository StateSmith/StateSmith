Follow [build instructions](./build-requirements.md) first.

This page will teach you to modify `ss.cli` project and run it.

> 🚩 `src/StateSmith.Cli/` should be your working directory during all of these instructions.

# Run `ss.cli`: Command Arguments
Run the command `dotnet run --framework=net9.0 --version`, after which you should see the output of `ss.cli`. 

It may prompt you about checking for updates first, but you will eventually see this:

```
StateSmith/src/StateSmith.Cli$ dotnet run --framework=net9.0 --version
<warnings...>
Using settings directory: /home/your-user/.config/StateSmith.Cli
StateSmith.Cli 0.0.0-local-build+0ae844cb6e61adcccfd108a40ff5513f25e81d13
<truncated...>
```

Why is the `--framework` arg needed? Because `ss.cli` targets multiple dotnet runtimes. You need to pick one that you have installed.

What does `--version` do? This arg is just an example and is passed through to `ss.cli`. You will usually pass `run`, `create` or `setup` here.


# Run `ss.cli`: Project Creation
Run the command `dotnet run --framework=net9.0 create` (this is just like `ss.cli create`).

You should see the StateSmith project creation menu:
```
── Workflow ───────────────────────────────────────────────────────────────────
Please select your ideal workflow. We recommend starting with "User Friendly".
You can manually add a .csx file later if you need more advanced features.

> User Friendly - generally recommended. Faster & simpler. (remembered)
  User Friendly - generally recommended. Faster & simpler.
  Advanced - adds a .csx file to support advanced features.
```


# Run `ss.cli`: Your First Modification
As your first test change, modify `src/StateSmith.Cli/Program.cs` by adding a custom print message at the top of the `Main` method:

```csharp
public static void Main(string[] args)
{
    Console.WriteLine("!!! MY CUSTOM STATESMITH CLI !!!");
```

Now run it again with `dotnet run --framework=net9.0` (automatically builds, so no need to do it manually) and you should see your message printed:

```
StateSmith/src/StateSmith.Cli$ dotnet run --framework=net9.0
<warnings...>
!!! MY CUSTOM STATESMITH CLI !!!
Using settings directory: /home/your-user/.config/StateSmith.Cli
StateSmith.Cli 0.0.0-local-build+dfdee5b9228e1107f3d8aebd0392884731d68bfd

Usage:
<truncated...>
```

# Run `ss.cli`: Advanced Usage

You can pass additional arguments which will get passed through, such as a path to your `csx` file:
```bash
dotnet run --framework=net9.0 run "/path/to/your-script.csx"
```

You can also directly invoke the compiled executable after running `dotnet build --framework=net9.0`. 

Usually this is not necessary, but mentioned here for good measure. This mostly allows to run it from a different working directory, in case your setup requires it due to exotic reasons:
```bash
/absolute/path/src/StateSmith.Cli/bin/Debug/net9.0/StateSmith.Cli run your-script.csx
```