#nullable enable

namespace StateSmith.Output.UserConfig;

public class RenderConfigBaseVars
{
    public string FileTop = "";

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/91
    /// </summary>
    public string AutoExpandedVars = "";

    /// <summary>
    /// Deafult variable expansion template.
    /// https://github.com/StateSmith/StateSmith/issues/284
    /// </summary>
    public string DefaultVarExpTemplate = "";

    /// <summary>
    /// Default function expansion template.
    /// https://github.com/StateSmith/StateSmith/issues/284
    /// </summary>
    public string DefaultFuncExpTemplate = "";

    /// <summary>
    /// Default variable and function expansion template.
    /// https://github.com/StateSmith/StateSmith/issues/284
    /// </summary>
    public string DefaultAnyExpTemplate = "";

    /// <summary>
    /// Not used yet. A comma separated list of allowed event names. TODO case sensitive?
    /// </summary>
    public string EventCommaList = "";

    public string VariableDeclarations = "";

    public string TriggerMap = "";

    public RenderConfigBaseVars()
    {
        
    }

    public void SetFrom(IRenderConfig config, bool autoDeIndentAndTrim)
    {
        string Process(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return "";

            if (autoDeIndentAndTrim)
                return StringUtils.DeIndentTrim(str);

            return str;
        }

        FileTop = Process(config.FileTop);
        AutoExpandedVars = Process(config.AutoExpandedVars);
        DefaultVarExpTemplate = Process(config.DefaultVarExpTemplate);
        DefaultFuncExpTemplate = Process(config.DefaultFuncExpTemplate);
        DefaultAnyExpTemplate = Process(config.DefaultAnyExpTemplate);

        VariableDeclarations = Process(config.VariableDeclarations);
        EventCommaList = Process(config.EventCommaList);
        TriggerMap = Process(config.TriggerMap);
    }

    public void CopyFrom(RenderConfigBaseVars otherConfig)
    {
        otherConfig.ErasePureCommentVarDecls();

        var SmartAppend = StringUtils.AppendInPlaceWithNewlineIfNeeded;

        SmartAppend(ref FileTop, otherConfig.FileTop);
        SmartAppend(ref VariableDeclarations, otherConfig.VariableDeclarations);
        SmartAppend(ref AutoExpandedVars, otherConfig.AutoExpandedVars);
        SmartAppend(ref DefaultVarExpTemplate, otherConfig.DefaultVarExpTemplate);
        SmartAppend(ref DefaultFuncExpTemplate, otherConfig.DefaultFuncExpTemplate);
        SmartAppend(ref DefaultAnyExpTemplate, otherConfig.DefaultAnyExpTemplate);
        SmartAppend(ref EventCommaList, otherConfig.EventCommaList);
        SmartAppend(ref TriggerMap, otherConfig.TriggerMap);

        ErasePureCommentVarDecls();
    }

    protected void ErasePureCommentVarDecls()
    {
        if (StringUtils.RemoveCCodeComments(VariableDeclarations).Trim().Length == 0)
        {
            VariableDeclarations = "";
        }
    }
}
