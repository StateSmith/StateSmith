#!/usr/bin/env dotnet-script
#r "nuget: StateSmith, 0.17.3"
using StateSmith.Output.UserConfig;
using StateSmith.Runner;

SmRunner runner = new(diagramPath: "lightbulb.puml", new LightSmRenderConfig(), transpilerId: TranspilerId.JavaScript);
runner.Run();


// This class gives StateSmith the info it needs to generate working code. This class can have any name.
public class LightSmRenderConfig : IRenderConfig
{
    string IRenderConfig.VariableDeclarations => """
        count: 0,            // a state machine variable
        switch_state: false, // an input to state machine
        light_state: false,  // an output from state machine
        """;
}
