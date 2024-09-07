using StateSmith.Output.UserConfig;
using StateSmith.Runner;

namespace StateSmith.Cli.Create;

public class CsxTemplateRenderer
{
    protected TemplateFilterEngine templateFilterEngine = new();
    protected TargetLanguageId targetLanguageId;
    protected string smName;
    protected string stateSmithVersion;
    protected string diagramPath;
    protected string template;

    public CsxTemplateRenderer(TargetLanguageId targetLanguageId, string stateSmithVersion, string diagramPath, string smName, string template = "")
    {
        this.targetLanguageId = targetLanguageId;
        this.stateSmithVersion = stateSmithVersion;
        this.diagramPath = diagramPath;
        this.template = template;
        this.smName = smName;
    }

    public void SetTemplate(string template)
    {
        this.template = template;
    }

    private static string ConvertPathsToForwardSlashes(string path)
    {
        return path.Replace('\\', '/');
    }

    public string Render()
    {
        diagramPath = ConvertPathsToForwardSlashes(diagramPath);  // we do this so that we don't need to escape backslashes in the generated string
        var str = template.Replace("{{stateSmithVersion}}", stateSmithVersion);
        str = str.Replace("{{diagramPath}}", diagramPath);
        str = str.Replace("{{smName}}", smName);

        TranspilerId transpilerId = targetLanguageId switch
        {
            TargetLanguageId.C => TranspilerId.C99,
            TargetLanguageId.CppC => TranspilerId.C99,
            TargetLanguageId.CSharp => TranspilerId.CSharp,
            TargetLanguageId.JavaScript => TranspilerId.JavaScript,
            TargetLanguageId.Java => TranspilerId.Java,
            _ => throw new System.NotImplementedException(),
        };
        str = str.Replace("{{transpilerId}}", transpilerId.ToString());

        var renderConfigBase = targetLanguageId switch
        {
            TargetLanguageId.C => nameof(IRenderConfigC),
            TargetLanguageId.CppC => nameof(IRenderConfigC),
            TargetLanguageId.CSharp => nameof(IRenderConfigCSharp),
            TargetLanguageId.JavaScript => nameof(IRenderConfigJavaScript),
            TargetLanguageId.Java => nameof(IRenderConfigJava),
            _ => throw new System.NotImplementedException(),
        };
        str = str.Replace("{{renderConfigBase}}", renderConfigBase);

        var langKeyFilterKey = targetLanguageId switch
        {
            TargetLanguageId.C => "C",
            TargetLanguageId.CppC => "CppC",
            TargetLanguageId.CSharp => "CSharp",
            TargetLanguageId.JavaScript => "JavaScript",
            TargetLanguageId.Java => "Java",
            _ => throw new System.NotImplementedException(),
        };

        str = templateFilterEngine.ProcessAllFilters(str, langKeyFilterKey);

        return str;
    }
}
