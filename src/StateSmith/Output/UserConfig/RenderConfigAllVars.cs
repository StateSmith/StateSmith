#nullable enable

namespace StateSmith.Output.UserConfig;

public class RenderConfigAllVars
{
    public readonly RenderConfigBaseVars Base;
    public readonly RenderConfigCVars C;
    public readonly RenderConfigCSharpVars CSharp;
    public readonly RenderConfigJavaScriptVars JavaScript;

    public RenderConfigAllVars()
    {
        Base = new RenderConfigBaseVars();
        C = new RenderConfigCVars();
        CSharp = new RenderConfigCSharpVars();
        JavaScript = new RenderConfigJavaScriptVars();
    }
}
