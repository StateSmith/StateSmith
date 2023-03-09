using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using Xunit;

namespace StateSmithTest.Output.Gil;

public class GilToCSharpTests
{
    /// <summary>
    /// Test for https://github.com/StateSmith/StateSmith/issues/122
    /// </summary>
    [Fact]
    public void DontRequireNameSpace()
    {
        SmRunner runner = new(diagramPath: "CSharpNoNameSpaceExampleSm.drawio.svg", new MyGlueLogic(), transpilerId: TranspilerId.CSharp);
        runner.GetExperimentalAccess().Settings.propagateExceptions = true;
        //runner.GetExperimentalAccess().Settings.dumpGilCodeOnError = true;
        runner.Run();
    }

    public class MyGlueLogic : IRenderConfigCSharp
    {
        //string IRenderConfigCSharp.NameSpace => "MySmNs";
    }
}
