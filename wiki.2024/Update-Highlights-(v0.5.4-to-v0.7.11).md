Hi everyone,

It's been about 5 months since StateSmith was initially released and we've got a lot of awesome improvements!


![image](https://user-images.githubusercontent.com/274012/218263552-cd4d1510-8824-496a-afe1-896f61cd0289.png)
![image](https://user-images.githubusercontent.com/274012/218209851-c904835e-0989-40ce-8868-4f1ac964cbe8.png)
![image](https://user-images.githubusercontent.com/274012/218264022-e29afff0-7f42-4159-bb7b-a0818be5b972.png)

🎉 The biggest news is that we now have `draw.io` support and a plugin for it! It's working really well!

⭐ Collaboration ready - Join us on [discord](https://discord.com/channels/1056394875278462996/1056394875278462999). StateSmith's core has been significantly cleaned up and is ready for collaboration. I'll be spending more time on [documentation](https://github.com/StateSmith/StateSmith/wiki) and [creating videos](https://www.youtube.com/@statesmith).

💻The other massive improvement is that the workflow is way nicer with `dotnet script`.

*Details follow...*

<br/>

---

<br/>



# Migration Guide
A few namespaces were refactored that will affect user code generation scripts. Here is how to update your existing projects:
  - replace `using StateSmith.Compiling;` with `using StateSmith.SmGraph;`
  - replace `using StateSmith.compiling;` with `using StateSmith.SmGraph;`
  - replace `using StateSmith.output;` with `using StateSmith.Output`

Changes like these are detailed in the [changelog](https://github.com/StateSmith/StateSmith/blob/main/CHANGELOG.md). Search for `BREAKING-CHANGES`.

Now onto the fun stuff!

<br/>







# draw.io
You can use draw.io online, the offline app, or even draw.io for confluence. That said, you'll get the best experience using the vscode draw.io extension with the StateSmith plugin. You can now do everything from a single editor! Design your state machine, run the StateSmith code gen, compile and test.

![image](https://user-images.githubusercontent.com/274012/218214303-0e81dff4-af95-4e94-80fc-f41040c34e90.png)

It also (very excitedly) opens the door for us to add features I thought would take a long time to get to. Things like a state machine simulator or even live visual tracing of a state machine running on embedded hardware.

[More info.](https://github.com/StateSmith/StateSmith/wiki/Getting-started-using-draw.io-with-StateSmith)

<br/>





# Collaboration Ready
![image](https://user-images.githubusercontent.com/274012/218209851-c904835e-0989-40ce-8868-4f1ac964cbe8.png)

Join us on [discord](https://discord.com/channels/1056394875278462996/1056394875278462999) or [github discussions](https://github.com/StateSmith/StateSmith/discussions).

The initial release of StateSmith didn't have a core code base that was ready for collaboration, but it is pretty decent right now. The internals have been heavily refactored and are ready for contributors to start hacking on and improving.  I'll be shifting my focus from developing features on my own to supporting contributors, [documentation](https://github.com/StateSmith/StateSmith/wiki) and [creating videos](https://www.youtube.com/@statesmith).

[More info](https://github.com/StateSmith/StateSmith/wiki/Contributing)

<br/>





# C# scripts
C# script files (*.csx) are another huge improvement. They allow user StateSmith scripts to be much less intrusive in your projects.

Instead of having to create another directory and C# solution and project, you can create a single small C# script file like shown below:

![image](https://user-images.githubusercontent.com/274012/218100717-b825ed2e-3c16-4036-b248-6e90bf6ee07f.png)

This is great because it allows the code generation script to live directly beside the design files and generated .c/.h file.

[More info.](https://github.com/StateSmith/StateSmith/wiki/Using-c%23-script-files-(.CSX)-instead-of-solutions-and-projects)

<br/>



# Simplified API
If you aren't using any advanced features of StateSmith, this is all you need:
```C#
#!/usr/bin/env dotnet-script
#r "nuget: StateSmith, 0.7.11-alpha" // specifies the version of StateSmith that will be used
using StateSmith.Runner;

SmRunner runner = new(diagramPath: "MySm.drawio.svg");
runner.Run();
```

[Try it out here.](https://github.com/StateSmith/example-drawio-1)

<br/>




# Support User Customization
StateSmith now allows user code gen scripts to modify the StateMachine before the code generation step. This allows you to add custom features, validations, whatever you might need. [This example project](https://github.com/StateSmith/StateSmith-examples/tree/main/modding-logging) shows how to easily add custom logging to specific states.

![image](https://user-images.githubusercontent.com/274012/218220287-9cdaf1ec-99e4-4581-9f76-0478328639cd.png)

[More info.](https://github.com/StateSmith/StateSmith-examples/tree/main/modding-logging)

<br/>




# Diagram Based RenderConfig
This comes directly from a user suggestion.
> it would be great if all editing could be done in the graph editor - e.g. inject variable definitions and header content via some purpose built init node. a lot of my embedded projects use a single struct holding most variables used elsewhere in the application - being able to define and reference such a struct without ever leaving the graph editor would be nice.

![image](https://user-images.githubusercontent.com/274012/218219928-61702f9a-ba4a-4aa7-9e5e-bb31ddffe7d8.png)

[More info.](https://github.com/StateSmith/StateSmith/wiki/Diagram-Based-Render-Config)

<br/>





# $mod prefix
This is a really handy feature for deeply nested state machines. It allows you to easily add prefixes to state names so that they are all unique.

![image](https://user-images.githubusercontent.com/274012/218220832-27f966ad-105e-4bb1-9484-4a92332b4341.png) 
![image](https://user-images.githubusercontent.com/274012/218220956-b65ec920-1912-48c1-8f6f-d8d8cd403729.png)

[More info.](https://github.com/StateSmith/StateSmith/wiki/$mod-prefix)

<br/>





# History States - Shallow, Deep, Custom
This comes directly from a [user suggestion](https://github.com/StateSmith/StateSmith/issues/56).

![image](https://user-images.githubusercontent.com/274012/218223084-b3c06acf-b3b2-4569-981b-9f86769aecae.png)

[More info.](https://github.com/StateSmith/StateSmith/blob/b3694bc2f725e1db89573f719eac21e9e1a1a363/docs/history-vertex.md)

<br/>





# Choice Pseudo States
StateSmith now supports Choice Pseudo States / Choice Points with else.

![image](https://user-images.githubusercontent.com/274012/218222587-8ca3817f-f6d5-4c72-a35c-a7a4d29ab9cd.png)

[More info.](https://github.com/StateSmith/StateSmith/wiki/Choice-Pseudo-States)

<br/>





# Initial States, Entry Points and Exit Points now behave like Choice Points
StateSmith 0.5.9 had a limitation of a single transition for initial states, entry points, exit points, but this has been lifted. These pseudo states now behave a lot like Choice Points.

![image](https://user-images.githubusercontent.com/274012/218222633-b46e22b4-c6f6-47e8-a9d7-02cadd4bab99.png)

[More info.](https://github.com/StateSmith/StateSmith/wiki/Choice-Pseudo-States)

<br/>





# Deep User Customization
The core of StateSmith has been modified to use Dependency Injection so that users can swap in their own code generation classes, or customize almost any aspect of how StateSmith works.

More documentation will be coming for this. If you are keen now, hit us up on [discord](https://discord.com/channels/1056394875278462996/1056394875278462999).

<br/>


# A few more too...
https://github.com/StateSmith/StateSmith/blob/main/CHANGELOG.md
