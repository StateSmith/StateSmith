namespace StateSmith.Output.UserConfig;

/// <summary>
/// NOTE! Field name used with reflection for toml parsing.
/// </summary>
public class RenderConfigCSharpVars
{
    public string NameSpace = "";
    public string Usings = "";
    public string ClassCode = "";

    /// <summary>
    /// See <see cref="IRenderConfigCSharp.BaseList"/>
    /// </summary>
    public string BaseList = "";
    public bool UseNullable = true;
    public bool UsePartialClass = true;

    public void SetFrom(IRenderConfigCSharp config, bool autoDeIndentAndTrim)
    {
        string Process(string str)
        {
            if (str.Trim().Length == 0)
                return "";

            if (autoDeIndentAndTrim)
                return StringUtils.DeIndentTrim(str);

            return str;
        }

        NameSpace = Process(config.NameSpace);
        Usings = Process(config.Usings);
        ClassCode = Process(config.ClassCode);
        UseNullable = config.UseNullable;
        UsePartialClass = config.UsePartialClass;
        BaseList = config.BaseList;
    }
}
