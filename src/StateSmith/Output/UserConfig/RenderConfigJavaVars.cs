namespace StateSmith.Output.UserConfig;

public class RenderConfigJavaVars
{
    public string Package = "";
    public string Imports = "";
    public string Extends = "";
    public string Implements = "";
    public string ClassCode = "";

    public void SetFrom(IRenderConfigJava config, bool autoDeIndentAndTrim)
    {
        string Process(string str)
        {
            if (str.Trim().Length == 0)
                return "";

            if (autoDeIndentAndTrim)
                return StringUtils.DeIndentTrim(str);

            return str;
        }

        Package = Process(config.Package);
        Imports = Process(config.Imports);
        Extends = Process(config.Extends);
        Implements = Process(config.Implements);
        ClassCode = Process(config.ClassCode);
    }
}
