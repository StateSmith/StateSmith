#!/usr/bin/env dotnet-script
#r "nuget: StateSmith, 0.17.3"
using StateSmith.Output.UserConfig;
using StateSmith.Runner;

SmRunner runner = new(diagramPath: "lightbulb.puml", new LightSmRenderConfig(), transpilerId: TranspilerId.JavaScript);
runner.Run();


// This class gives StateSmith the info it needs to generate working code. This class can have any name.
public class LightSmRenderConfig : IRenderConfig
{
    // `FileTop` text will appear at the top of the generated file. Use for comments, copyright notices, code...
    string IRenderConfig.FileTop => """
        // Copyright: turtles, turtles, turtles...
        // You can include other files/modules specific to your programming language here
        let x = 55; // You can even output raw code...
        """;
}
