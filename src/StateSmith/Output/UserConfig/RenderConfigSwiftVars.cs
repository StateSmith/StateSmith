namespace StateSmith.Output.UserConfig;

public class RenderConfigSwiftVars
{
    public string Imports = "";
    public string BaseList = "";
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
        BaseList = Process(config.BaseList);
        ClassCode = Process(config.ClassCode);
    }
}
