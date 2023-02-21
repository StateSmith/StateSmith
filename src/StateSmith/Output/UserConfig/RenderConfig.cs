namespace StateSmith.Output.UserConfig;

public class RenderConfig
{
    public string FileTop = "";

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/91
    /// </summary>
    public string AutoExpandedVars = "";

    /// <summary>
    /// Not used yet. A comma separated list of allowed event names. TODO case sensitive?
    /// </summary>
    public string EventCommaList = "";

    public string VariableDeclarations = "";

    public void CopyFrom(RenderConfig otherConfig)
    {
        otherConfig.IgnorePureCommentVarDecls();

        static void SmartAppend(ref string str, string toAppend)
        {
            str = StringUtils.AppendWithNewlineIfNeeded(str, toAppend);
        }

        SmartAppend(ref FileTop, otherConfig.FileTop);
        SmartAppend(ref VariableDeclarations, otherConfig.VariableDeclarations);
        SmartAppend(ref AutoExpandedVars, otherConfig.AutoExpandedVars);
        SmartAppend(ref EventCommaList, otherConfig.EventCommaList);

        IgnorePureCommentVarDecls();
    }

    protected void IgnorePureCommentVarDecls()
    {
        if (StringUtils.RemoveCCodeComments(VariableDeclarations).Trim().Length == 0)
        {
            VariableDeclarations = "";
        }
    }
}
