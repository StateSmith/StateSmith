#nullable enable

using StateSmith.Output.UserConfig;

namespace StateSmithTest.Output.UserConfig;

public class RenderConfigFields : IRenderConfig
{
    public string FileTop { get; set; } = "";
    public string AutoExpandedVars { get; set; } = "";
    public string DefaultVarExpTemplate { get; set; } = "";
    public string DefaultFuncExpTemplate { get; set; } = "";
    public string DefaultAnyExpTemplate { get; set; } = "";
    public string EventCommaList { get; set; } = "";
    public string VariableDeclarations { get; set; } = "";
    public string TriggerMap { get; set; } = "";
}
