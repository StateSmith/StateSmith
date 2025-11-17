using StateSmith.Common;

namespace StateSmith.Output.UserConfig;

public class RenderConfigBerryVars
{
    public string Imports = "";
    public string Extends = "";
    public string ClassCode = "";

    public void SetFrom(IRenderConfigBerry config, bool autoDeIndentAndTrim)
    {
        string Process(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return "";

            if (autoDeIndentAndTrim)
                return StringUtils.DeIndentTrim(str);

            return str;
        }

        Imports = Process(config.Imports);
        Extends = config.Extends.Trim();
        ClassCode = Process(config.ClassCode);
    }
}
