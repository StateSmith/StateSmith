Let's say you want to use a custom built StateSmith lib with one of your C# script .csx files.

How would you do it?

## Step 1 - build the lib
Run `dotnet build` in `src/StateSmith` directory:
```
src/StateSmith$ dotnet build
Restore complete (0.4s)
  StateSmith succeeded (0.2s) → bin/Debug/net6.0/StateSmith.dll

Build succeeded in 1.0s
src/StateSmith$ 
```

Pay attention to where the .dll was created. For me, it was created at `src/StateSmith/bin/Debug/net6.0/StateSmith.dll`.

## Step 2 - update your .csx file `#r`
Let's say this is your example .csx file

```cs
#!/usr/bin/env dotnet-script
#r "nuget: StateSmith, 0.9.10-alpha" // this line specifies which version of StateSmith to use and download from c# nuget web service.

using StateSmith.Runner;

SmRunner runner = new(diagramPath: "ButtonLightSm.drawio.svg", transpilerId: TranspilerId.C99);
// ...snip...
```

Modify the reference `#r` line to use the path to your custom built .dll.

```diff
- #r "nuget: StateSmith, 0.9.10-alpha" // put back in when done testing with your .dll
+ #r "/home/afk/code/StateSmith/src/StateSmith/bin/Debug/net6.0/StateSmith.dll" // use your own absolute or relative path to your dll
```

## Step 3 - run with `--no-cache`
You will likely want to run with the `--no-cache` argument. Otherwise, dotnet-script will likely cache your .dll after the first time and keep that since you likely aren't changing the StateSmith lib version meta data.

Example:
```
dotnet-script ./your-script.csx --no-cache
```

