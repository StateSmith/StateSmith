All of the info here is about how to setup vscode to debug C# script files (.csx) and provide intellisense for them.

This is an optional step, but can be very helpful when developing your .csx files.



# Install
1. If you haven't installed `dotnet-script` yet, see [this page](https://github.com/StateSmith/StateSmith/wiki/StateSmith-install-requirements#install-dotnet-script).
2. Install the official C# [vscode extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp).
    * **DO NOT** install the vscode C# `Dev Kit` extension.
3. Re-enable omnisharp (see directly below).

## 📢 NOTE! 📢
A recent vscode c# extension update removed omnisharp (which we need).
It's easy to re-enable though: https://github.com/StateSmith/StateSmith/issues/221

<br>

# Improved Setup!
This is now much easier with the [StateSmith.Cli tool](https://github.com/StateSmith/StateSmith/blob/main/src/StateSmith.Cli/README.md).

<br>
<br>
<br>
<br>

# Manual Setup
In your vscode project directory, run the below command.

```bash
dotnet script init delete_me_dummy_file.csx
```

This will update the `.vscode/launch.json` file to enable C# script debugging and code completion.

💡 TIP! You can improve the launch.json [file here](https://github.com/StateSmith/StateSmith/wiki/vscode-csx).

You can delete the `delete_me_dummy_file.csx` file afterwards.

```bash
rm ./delete_me_dummy_file.csx
```

It will also create an `omnisharp.json` file for vscode. The `omnisharp.json` file should normally not be committed to a repo (.gitignore) as different users may have different versions of dotnet installed.

![image](https://user-images.githubusercontent.com/274012/213771717-a4d4d498-758d-489e-8732-1f6169ca4d44.png)

## Restart omnisharp
Now use the vscode command pallette to run `OmniSharp: Restart OmniSharp` or restart vscode and you should be good to go.

## Select omnisharp project
If your directory has other C# projects/solutions in it, you'll likely need to tell omnisharp to look at .csx files.

Run vscode command: `OmniSharp: Select Project` and select `CSX` for the correct directory.
See troubleshooting below for more details.


<br>

# Troubleshooting
## Troubleshooting - no intellisense for `.csx` files 
If you've done all the above steps, you might need to tell `OmniSharp` which project to select. 

Use the vscode command pallette to run `OmniSharp: Select Project` and select `CSX` for the correct directory.

This problem can happen if `OmniSharp` auto selects the a Visual Studio solution in the same directory.

![omnisharp-select-project](https://user-images.githubusercontent.com/274012/213774300-6ac92e36-a521-4387-9250-84a790896fd8.gif)

## Troubleshooting - can't upgrade StateSmith nuget
I've run into this a few times when I want to use the latest StateSmith nuget that was released within the last few minutes.

You might see an error message like this:

```
Unable to restore packages from 'C:\Users\your_user\AppData\Local\Temp\dotnet-script\C\Users\some_path\your_project_dir\.\src\net6.0\script.csproj'
Make sure that all script files contains valid NuGet references
```

It appears to be a caching issue. Run the below command to fix.
```
dotnet nuget locals http-cache --clear
```
More nuget tips: https://github.com/StateSmith/StateSmith/wiki/nuget-tips


## Troubleshooting - run isolated and in debug mode
Try `dotnet script <your_script_file>.csx --isolated-load-context --debug --no-cache`

See https://github.com/StateSmith/StateSmith/issues/123



<br>
<br>

# More csx info
https://github.com/StateSmith/StateSmith/wiki/csx
