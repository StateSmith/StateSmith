#nullable enable

using StateSmith.Runner;
using StateSmith.SmGraph;
using System.IO;

namespace StateSmith.Output;

public class SmDesignDescriber
{
    protected SmDesignDescriberSettings settings;
    protected IDiagramVerticesProvider diagramVerticesProvider;
    protected SmGraphDescriber smDescriber;

    public SmDesignDescriber(SmDesignDescriberSettings settings, IDiagramVerticesProvider diagramVerticesProvider, IOutputInfo outputInfo)
    {
        this.settings = settings;
        this.diagramVerticesProvider = diagramVerticesProvider;

        var filePath = $"{outputInfo.OutputDirectory}{outputInfo.BaseFileName}.md";
        using var writer = new StreamWriter(filePath);
        smDescriber = new SmGraphDescriber(writer);
    }

    public void DescribeBeforeTransformations()
    {

    }

    public void DescribeAfterTransformations()
    {

    }
}

