namespace StateSmith.Output.UserConfig;

public class RenderConfigAllVars
{
    public readonly RenderConfigBaseVars BaseVars;
    public readonly RenderConfigCVars CVars;
    public readonly RenderConfigCSharpVars CSharpVars;
    public readonly RenderConfigJavaScriptVars JavaScriptVars;

    public RenderConfigAllVars()
    {
        BaseVars = new RenderConfigBaseVars();
        CVars = new RenderConfigCVars();
        CSharpVars = new RenderConfigCSharpVars();
        JavaScriptVars = new RenderConfigJavaScriptVars();
    }
}
