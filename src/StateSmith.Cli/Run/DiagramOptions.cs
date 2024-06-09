using StateSmith.Runner;

namespace StateSmith.Cli.Run;

public class DiagramOptions
{
    public TranspilerId Lang;
    public bool NoSimGen;

    public DiagramOptions()
    {
        
    }

    public DiagramOptions(TranspilerId lang, bool noSimGen)
    {
        Lang = lang;
        NoSimGen = noSimGen;
    }

    public string Describe()
    {
        return $"Lang: {Lang}, NoSimGen: {NoSimGen}";
    }
}

