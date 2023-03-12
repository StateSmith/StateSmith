using StateSmithTest.spec._2.CSharp;

namespace Csharp.Spec2smTests;

public partial class Spec2SmBase
{
    // this is needed because TracingModder doesn't understand expansions yet
    // https://github.com/StateSmith/StateSmith/issues/128
    protected static void trace(string message) => MainClass.Trace(message);
}
