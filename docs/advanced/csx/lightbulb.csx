#r "nuget: StateSmith, 0.17.3"

using StateSmith.Runner;
SmRunner runner = new(diagramPath: "lightbulb.puml", transpilerId: TranspilerId.JavaScript);
runner.Run();