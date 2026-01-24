namespace StateSmith.Output.UserConfig;

public class RenderConfigSwiftVars
{
    public string Imports = "";
    public string Extends = "";
    public string ClassCode = "";

    public void SetFrom(IRenderConfigSwift config, bool autoDeIndentAndTrim)
    {
        string Process(string str)
        {
            if (str.Trim().Length == 0)
                return "";

            if (autoDeIndentAndTrim)
                return StringUtils.DeIndentTrim(str);

            return str;
        }

        Imports = Process(config.Imports);
        Extends = Process(config.Extends);
        ClassCode = Process(config.ClassCode);
    }
}
