namespace StateSmith.Output.UserConfig;

public class RenderConfigVars
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

    public string TriggerMap = "";

    public void SetFrom(IRenderConfig config, bool autoDeIndentAndTrim)
    {
        string Process(string str)
        {
            if (str.Trim().Length == 0)
                return "";

            if (autoDeIndentAndTrim)
                return StringUtils.DeIndentTrim(str);

            return str;
        }

        FileTop = Process(config.FileTop);
        VariableDeclarations = Process(config.VariableDeclarations);

        AutoExpandedVars = Process(config.AutoExpandedVars);
        EventCommaList = Process(config.EventCommaList);
        TriggerMap = Process(config.TriggerMap);
    }

    public void CopyFrom(RenderConfigVars otherConfig)
    {
        otherConfig.IgnorePureCommentVarDecls();

        var SmartAppend = StringUtils.AppendInPlaceWithNewlineIfNeeded;

        SmartAppend(ref FileTop, otherConfig.FileTop);
        SmartAppend(ref VariableDeclarations, otherConfig.VariableDeclarations);
        SmartAppend(ref AutoExpandedVars, otherConfig.AutoExpandedVars);
        SmartAppend(ref EventCommaList, otherConfig.EventCommaList);
        SmartAppend(ref TriggerMap, otherConfig.TriggerMap);

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
