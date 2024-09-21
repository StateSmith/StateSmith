namespace StateSmith.Output.UserConfig;

public class RenderConfigTypeScriptVars
{
    public string Extends = "";
    public string Implements = "";
    public string ClassCode = "";

    public void SetFrom(IRenderConfigTypeScript config, bool autoDeIndentAndTrim)
    {
        string Process(string str)
        {
            if (str.Trim().Length == 0)
                return "";

            if (autoDeIndentAndTrim)
                return StringUtils.DeIndentTrim(str);

            return str;
        }

        Extends = Process(config.Extends);
        Implements = Process(config.Implements);
        ClassCode = Process(config.ClassCode);
    }
}
