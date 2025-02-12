C# script files (*.csx) allow user scripts to be much less intrusive in your source code.

Instead of having to create another directory and C# solution and project, you can create a single small C# script file like shown below:

![image](https://user-images.githubusercontent.com/274012/218100717-b825ed2e-3c16-4036-b248-6e90bf6ee07f.png)

This is great because it allows the code generation script to live directly beside the design files and generated .c/.h file.

If you haven't installed `dotnet-script` yet, see https://github.com/StateSmith/StateSmith/wiki/StateSmith-install-requirements#install-dotnet-script

Regular C# solutions and projects will continue to be supported. At the end of the day, we just need a way of running a user's code generation program/script.

# Running a C# script file - cross platform
There are a few different ways to run the script file.

Using the command line (both are equivalent):
```console
dotnet-script Tutorial1Sm.csx
dotnet script Tutorial1Sm.csx
```

The first time you run the script, it will have a bit of a delay while it resolves any dependencies and compiles it.

## Running a C# script file - Linux Specific
If your .csx file starts with the proper shebang `#!/usr/bin/env dotnet-script` then you can execute it like any other Linux script:

![image](https://user-images.githubusercontent.com/274012/213876453-a3bd1ea8-856c-4f45-b479-84f67c01d73c.png)

Make sure your .csx file doesn't contain windows new line characters \r\n or you can get some weird error messages:

![image](https://user-images.githubusercontent.com/274012/213876487-68f56af4-aca8-465a-9dab-02cbff453254.png)

If you run into this, use the vscode command `Change End of Line Sequence` and select `LF`.

## Running a C# script file - Windows Specific
If you registered dotnet-script with `dotnet script register`, then you can also call the script file from the command line:

![image](https://user-images.githubusercontent.com/274012/213876578-fcb02716-240e-40ad-98cf-4f88e6ffd77a.png)

The downside is that it creates opens another terminal window. If you know how to prevent that, let me know.

You can also just double click the `.csx` file to run it like a .exe

![image](https://user-images.githubusercontent.com/274012/213876623-02a878cd-415f-4b86-a2e3-b352d0ce98a1.png)

If you plan on running the script by double clicking, I would recommend using the following code to have your script wait for a key press before closing so that you can see the output:

```c#
// Waiting for a key press can be handy if this script was launched by double clicking.
// This will give you a chance to see result before the console window closes.
// Feel free to remove.
static void MaybeWaitForKeyPress()
{
    if (!Debugger.IsAttached && !Console.IsInputRedirected && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        Console.WriteLine("Press any key to finish");
        Console.ReadKey(); // only do if not debugging because vscode debugging will throw at this line
    }
}
```

<br>
<br>
<br>
<br>

# The below information has moved. 
See https://github.com/StateSmith/StateSmith/wiki/vscode-csx

Keeping below headers to not break any existing links to these page sections.

### Setup vscode for debugging C# script files and intellisense
See https://github.com/StateSmith/StateSmith/wiki/vscode-csx
### Troubleshooting
See https://github.com/StateSmith/StateSmith/wiki/vscode-csx
### Troubleshooting - no intellisense for `.csx` files 
See https://github.com/StateSmith/StateSmith/wiki/vscode-csx
### Troubleshooting - can't upgrade StateSmith nuget
See https://github.com/StateSmith/StateSmith/wiki/vscode-csx
### Troubleshooting - run isolated and in debug mode
See https://github.com/StateSmith/StateSmith/wiki/vscode-csx