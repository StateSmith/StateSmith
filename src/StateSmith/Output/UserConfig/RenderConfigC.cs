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

    public void SetFromIRenderConfigC(IRenderConfigC config, bool autoDeIndentAndTrim)
    {
        string Process(string str)
        {
            if (str.Trim().Length == 0)
                return "";

            if (autoDeIndentAndTrim)
                return StringUtils.DeIndentTrim(str);

            return str;
        }

        HFileTop = Process(config.HFileTop);
        HFileIncludes = Process(config.HFileIncludes);

        CFileTop = Process(config.CFileTop);
        CFileIncludes = Process(config.CFileIncludes);

        VariableDeclarations = Process(config.VariableDeclarations);
        AutoExpandedVars = Process(config.AutoExpandedVars);

        EventCommaList = Process(config.EventCommaList);
        IgnorePureCommentVarDecls();
    }

    public void CopyFrom(RenderConfigC otherConfig)
    {
        otherConfig.IgnorePureCommentVarDecls();

        static void SmartAppend(ref string str, string toAppend)
        {
            str = StringUtils.AppendWithNewlineIfNeeded(str, toAppend);
        }

        SmartAppend(ref HFileTop, otherConfig.HFileTop);
        SmartAppend(ref HFileIncludes, otherConfig.HFileIncludes);
        SmartAppend(ref CFileTop, otherConfig.CFileTop);
        SmartAppend(ref CFileIncludes, otherConfig.CFileIncludes);
        SmartAppend(ref VariableDeclarations, otherConfig.VariableDeclarations);
        SmartAppend(ref AutoExpandedVars, otherConfig.AutoExpandedVars);
        SmartAppend(ref EventCommaList, otherConfig.EventCommaList);

        IgnorePureCommentVarDecls();
    }

    private void IgnorePureCommentVarDecls()
    {
        if (StringUtils.RemoveCCodeComments(VariableDeclarations).Trim().Length == 0)
        {
            VariableDeclarations = "";
        }
    }
}
