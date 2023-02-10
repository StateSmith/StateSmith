#!/usr/bin/env dotnet-script
// This is a c# script file. See https://github.com/StateSmith/StateSmith/wiki/Using-c%23-script-files-(.CSX)-instead-of-solutions-and-projects

#r "nuget: StateSmith, 0.7.11-alpha" // this line specifies which version of StateSmith to use and download from c# nuget web service.

// spell-checker: ignore drawio

using StateSmith.Input.Expansions;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;

SmRunner runner = new(diagramPath: "BlankTemplateSm.drawio.svg", new MyGlueLogic());
runner.Run();

#pragma warning disable IDE1006, CA1050  // ignore C# guidelines for script stuff below

/// <summary>
/// This class gives StateSmith the info it needs to generate working C code.
/// It adds user code to the generated .c/.h files, declares user variables,
/// and provides diagram code expansions. This class can have any name.
/// </summary>
public class MyGlueLogic : IRenderConfigC
{
    // You can define RenderConfig options in the diagram now if you like.
    // See https://github.com/StateSmith/StateSmith/issues/23
    string IRenderConfigC.HFileIncludes => @"
    ";

    /// <summary>
    /// This nested class creates expansions because it is inside a class that implements `IRenderConfigC`.
    /// This class can have any name.
    /// </summary>
    public class MyExpansions : UserExpansionScriptBase
    {
    }
}
