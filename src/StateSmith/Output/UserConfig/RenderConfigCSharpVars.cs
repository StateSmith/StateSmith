namespace StateSmith.Output.UserConfig;

public class RenderConfigCSharpVars
{
    public string NameSpace = "";
    public string Usings = "";
    public string ClassCode = "";
    public bool UseNullable = true;

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
    }
}
