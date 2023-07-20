#nullable enable

using StateSmith.Runner;
using StateSmith.SmGraph;
using System.IO;
using System.Text;

namespace StateSmith.Output;

public class SmDesignDescriber
{
    protected SmDesignDescriberSettings settings;
    protected IDiagramVerticesProvider diagramVerticesProvider;
    protected SmGraphDescriber smDescriber;

    public SmDesignDescriber(SmDesignDescriberSettings settings, IDiagramVerticesProvider diagramVerticesProvider)
    {
        this.settings = settings;
        this.diagramVerticesProvider = diagramVerticesProvider;

        using var writer = new StreamWriter(filePath);
        var describer = new SmGraphDescriber(writer);

    }

    public void DescribeBeforeTransformations()
    {
        var sb = new StringBuilder();

        SmGraphDescriber smDescriber = new(new StringWriter(sb));

    }

    public void DescribeAfterTransformations()
    {

    }
}

