#!/usr/bin/env dotnet-script
#r "nuget: StateSmith, {{stateSmithVersion}}"
SmRunner runner = new(diagramPath: "{{diagramPath}}", new MyRenderConfig(), transpilerId: TranspilerId.{{transpilerId}});
