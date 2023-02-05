namespace StateSmith.Output.UserConfig;

public class RenderConfigC
{
    /// <summary>
    /// Whatever this property returns will be placed at the top of the rendered .h file.
    /// </summary>
    public string HFileTop = "";

    public string HFileIncludes = "";

    /// <summary>
    /// Whatever this property returns will be placed at the top of the rendered .c file.
    /// </summary>
    public string CFileTop = "";

    public string CFileIncludes = "";

    public string VariableDeclarations = "";

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/91
    /// </summary>
    public string AutoExpandedVars = "";

    /// <summary>
    /// Not used yet. A comma separated list of allowed event names. TODO case sensitive?
    /// </summary>
    public string EventCommaList = "";

    public void SetFromIRenderConfigC(IRenderConfigC config)
    {
        HFileTop = config.HFileTop;
        HFileIncludes = config.HFileIncludes;

        CFileTop = config.CFileTop;
        CFileIncludes= config.CFileIncludes;
     
        VariableDeclarations = config.VariableDeclarations;
        AutoExpandedVars = config.AutoExpandedVars;
     
        EventCommaList = config.EventCommaList;
    }
}
