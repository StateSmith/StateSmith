> This page assumes that you understand the basics covered in [tutorial-2](https://github.com/StateSmith/tutorial-2).

## Improved!
This is now much easier with the [StateSmith.Cli tool](https://github.com/StateSmith/StateSmith/blob/main/src/StateSmith.Cli/README.md).

<br>
<br>
<br>

## 1. Install or setup plugin
See [StateSmith-drawio-plugin/wiki](https://github.com/StateSmith/StateSmith-drawio-plugin/wiki/).



## 2. Add draw.io diagram
StateSmith recognizes the following file extensions as draw.io files:
* `.drawio.svg` (encoded xml)
* `.drawio` (regular xml)
* `.dio` (regular xml)

Which to choose? See page [draw.io file choice](https://github.com/StateSmith/StateSmith/wiki/draw.io:-file-choice).


## 3. Add C# code gen script
1. Create a C# script file called something like `code_gen.csx` with the below contents. 
2. Modify it for your diagram file path.
3. Modify it for your [TranspilerId](https://github.com/StateSmith/StateSmith/blob/main/src/StateSmith/Runner/TranspilerId.cs): `TranspilerId.C99`, `TranspilerId.CSharp`, `TranspilerId.JavaScript`

```c#
#!/usr/bin/env dotnet-script
#r "nuget: StateSmith, 0.9.9-alpha"

using StateSmith.Common;
using StateSmith.Input.Expansions;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;

// See https://github.com/StateSmith/tutorial-2/blob/main/lesson-1/
SmRunner runner = new(diagramPath: "<your-diagram-file-path>", new MyRenderConfig(), transpilerId: <your-chosen-transpiler>);
runner.Run();

// See https://github.com/StateSmith/tutorial-2/tree/main/lesson-2
public class MyRenderConfig : IRenderConfig
{
    // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-3
    public class MyExpansions : UserExpansionScriptBase
    {
    }
}
```

## 4. Add intellisense?
```
dotnet-script init
```
[More info here.](https://github.com/StateSmith/StateSmith/wiki/Using-c%23-script-files-(.CSX)-instead-of-solutions-and-projects)