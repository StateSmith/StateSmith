#nullable enable

using StateSmith.Input.Settings;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using StateSmith.SmGraph.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateSmith.SmGraph;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/335
/// </summary>
public class TomlConfigVerticesProcessor
{
    TomlReader tomlReader;

    public TomlConfigVerticesProcessor(RenderConfigAllVars renderConfigAllVars, RunnerSettings smRunnerSettings)
    {
        tomlReader = new TomlReader(renderConfigAllVars, smRunnerSettings);
    }

    public void Process(StateMachine sm)
    {
        // we gather into a list first because we are modifying the graph
        List<ConfigOptionVertex> toProcess = new();

        sm.VisitTypeRecursively<ConfigOptionVertex>(v =>
        {
            if (v.name.Equals("toml", StringComparison.OrdinalIgnoreCase))
            {
                toProcess.Add(v);
            }
        });

        foreach (var configOptionVertex in toProcess)
        {
            // these vertices are not allowed to have children
            if (configOptionVertex.Children.Any())
            {
                throw new VertexValidationException(configOptionVertex, "toml config vertices cannot have children");
            }

            tomlReader.Read(configOptionVertex.value);
            configOptionVertex.RemoveChildrenAndSelf();
        }
    }
}
