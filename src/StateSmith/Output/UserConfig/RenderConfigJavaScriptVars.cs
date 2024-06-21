using StateSmith.Common;

namespace StateSmith.Output.UserConfig;

/// <summary>
/// NOTE! Field name used with reflection for toml parsing.
/// </summary>
public class RenderConfigJavaScriptVars
{
    public string ExtendsSuperClass = "";
    public string ClassCode = "";
    public bool UseExportOnClass = false;

    /// <summary>
    /// Set to "#" if you want to support new js versions.
    /// Set to "_" if you want to support old js versions.
    /// </summary>
    public string PrivatePrefix = "#";

    public void SetFrom(IRenderConfigJavaScript config, bool autoDeIndentAndTrim)
    {
        string Process(string str)
        {
            if (str.Trim().Length == 0)
                return "";

            if (autoDeIndentAndTrim)
                return StringUtils.DeIndentTrim(str);

            return str;
        }

        ClassCode = Process(config.ClassCode);
        PrivatePrefix = Process(config.PrivatePrefix);

        ExtendsSuperClass = config.ExtendsSuperClass.Trim();
        UseExportOnClass = config.UseExportOnClass;
    }
}
